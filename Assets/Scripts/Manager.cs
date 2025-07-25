using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Manager : MonoBehaviour
{
    private bool gameStarted = false;
    [Header("Game Settings")]
    [Tooltip("The maximum age of a star in seconds, after which it will change state")]
    [SerializeField] private float starMaxAge = 1800f;
    public float StarMaxAge => starMaxAge;
    [Tooltip("The maximum age of a red giant star in seconds, after which it will change state")]
    [SerializeField] private float redGiantMaxAge = 60f;
    public float RedGiantMaxAge => redGiantMaxAge;
    [Tooltip("Default supernova force (will be scaled)")]
    [SerializeField] private float supernovaForce = 50f;
    public float SupernovaForce => supernovaForce;
    [Tooltip("Time in seconds between projection appearing and event")]
    [SerializeField] private float projectionTime = 45f;
    public float ProjectionTime => projectionTime;
    [Tooltip("Max time after max star age + max red giant age")]
    [SerializeField] private float maxTimeAfterAllStarsDied = 0f;
    public float MaxTimeAfterAllStarsDied => maxTimeAfterAllStarsDied;

    [Header("Time Distortion Settings")]
    [Tooltip("Normal time distortion unit")]
    [SerializeField] private float normalUnit = 1f;
    [Tooltip("Accelerated time distortion unit")]
    [SerializeField] private float acceleratedUnit = 3f;

    [Tooltip("Time distortion multiplier")]
    [SerializeField] private float timeFactor = 5f;
    [Tooltip("Orbit distortion multiplier")]
    [SerializeField] private float orbitFactor = 2f;

    // Formula: add up the units contributed by all players (backwards would be negative), then multiply the result by the factor
    // The result is the multiplier applied to Time.deltaTime

    [Header("Other")]
    [Tooltip("Prefabs")]
    [SerializeField] private LoadedPrefabs loadedPrefabs;
    public LoadedPrefabs LoadedPrefabs => loadedPrefabs;
    private static Manager instance;
    public static Manager Instance => instance;

    [Tooltip("Player spawn")]
    [SerializeField] private Transform playerSpawn;
    public Transform PlayerSpawn => playerSpawn;
    [SerializeField] private float playerSpawnRadius = 3f;
    public float PlayerSpawnRadius => playerSpawnRadius;

    [Tooltip("Time between hot red giant and nova")]
    [SerializeField] private float hotGiantTransitionGap = 5f;
    public float HotGiantTransitionGap => hotGiantTransitionGap;

    [Tooltip("Hot red giant transition time")]
    [SerializeField] private float hotTransitionTime = 3f;
    public float HotTransitionTime => hotTransitionTime;

    [Tooltip("Powerups")]
    [SerializeField] private List<PowerupData> powerups = new List<PowerupData>();
    public List<PowerupData> Powerups => powerups;

    private float gameTime = 0f;
    public float GameTime => gameTime;
    public event Action<float> GameTimeUpdate;

    private float orbitTimeDisplacement = 0f;
    public float OrbitTimeDisplacement => orbitTimeDisplacement;
    public float OrbitTime => gameTime + orbitTimeDisplacement;
    // should trigger on game time updates and orbit time displacement changes
    public event Action<float> OrbitTimeUpdate;

    private float maxNaturalTime = 0f;
    private int playerTurn = 0;

    private Dictionary<ulong, Powerup> selectedPowerups = new Dictionary<ulong, Powerup>();
    private Dictionary<ulong, List<Powerup>> playerPowerups = new Dictionary<ulong, List<Powerup>>();

    public enum TimeDistortionType
    {
        Time,
        Orbit
    }
    public struct TimeDistortion : INetworkSerializable
    {
        public TimeDistortionType type;
        public bool accelerated;
        public bool forward;

        public TimeDistortion(TimeDistortionType type, bool accelerated, bool forward)
        {
            this.type = type;
            this.accelerated = accelerated;
            this.forward = forward;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref type);
            serializer.SerializeValue(ref accelerated);
            serializer.SerializeValue(ref forward);
        }
    }

    public bool DistortionActive => playerTimeDistortions.Count > 0;

    private List<ulong> playerIds = new List<ulong>();
    private Dictionary<ulong, PlayerBall> playerBalls = new Dictionary<ulong, PlayerBall>();
    private List<ulong> victoriousPlayers = new List<ulong>();
    private Dictionary<ulong, TimeDistortion> playerTimeDistortions = new Dictionary<ulong, TimeDistortion>();
    private float NetTimeDistortionFactor
    {
        get
        {
            float timeDistortionUnits = 0;
            foreach (TimeDistortion timeDistortion in playerTimeDistortions.Values)
            {
                if (timeDistortion.type == TimeDistortionType.Orbit) continue;
                if (timeDistortion.forward)
                {
                    timeDistortionUnits += timeDistortion.accelerated ? acceleratedUnit : normalUnit;
                }
                else
                {
                    timeDistortionUnits -= timeDistortion.accelerated ? acceleratedUnit : normalUnit;
                }
            }
            return timeDistortionUnits * timeFactor;
        }
    }
    private float NetOrbitDistortionFactor
    {
        get
        {
            float timeDistortionUnits = 0;
            foreach (TimeDistortion timeDistortion in playerTimeDistortions.Values)
            {
                if (timeDistortion.type == TimeDistortionType.Time) continue;
                if (timeDistortion.forward)
                {
                    timeDistortionUnits += timeDistortion.accelerated ? acceleratedUnit : normalUnit;
                }
                else
                {
                    timeDistortionUnits -= timeDistortion.accelerated ? acceleratedUnit : normalUnit;
                }
            }
            return timeDistortionUnits * orbitFactor;
        }
    }

    private void Awake()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Destroy(this);
            return;
        }
        instance = this;
        GameTimeUpdate += (time) => OrbitTimeUpdate?.Invoke(time + orbitTimeDisplacement);
    }

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        maxNaturalTime = starMaxAge + redGiantMaxAge + maxTimeAfterAllStarsDied;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            playerIds.Add(clientId);
            selectedPowerups[clientId] = null;
            playerPowerups[clientId] = new List<Powerup>();

            GameObject player = Instantiate(loadedPrefabs.PlayerBall, playerSpawn.position + (Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)) * (UnityEngine.Random.Range(0, playerSpawnRadius) * Vector2.right)), Quaternion.identity);
            playerBalls.Add(clientId, player.GetComponent<PlayerBall>());
            ServerSidePlayerSetup(player.GetComponent<PlayerBall>(), clientId);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;
        ServerSideStartGame();
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer || !gameStarted) return;
        if (DistortionActive)
        {
            gameTime = Mathf.Clamp(gameTime + (NetTimeDistortionFactor * Time.deltaTime), 0, maxNaturalTime);
            orbitTimeDisplacement += NetOrbitDistortionFactor * Time.deltaTime;
            GameTimeUpdate?.Invoke(gameTime); // updates orbit time as well

            //switch (timeDistortion.type)
            //{
            //    case TimeDistortionType.Time:
            //        if (timeDistortion.forward)
            //        {
            //            gameTime += timeDistortion.accelerated ? timeForwardAcceleratedFactor * Time.deltaTime : timeForwardFactor * Time.deltaTime;
            //            gameTime = Mathf.Min(gameTime, maxNaturalTime);
            //        }
            //        else
            //        {
            //            gameTime -= timeDistortion.accelerated ? timeBackwardAcceleratedFactor * Time.deltaTime : timeBackwardFactor * Time.deltaTime;
            //            gameTime = Mathf.Max(gameTime, 0f);
            //        }
            //        GameTimeUpdate?.Invoke(gameTime);
            //        break;
            //    case TimeDistortionType.Orbit:
            //        if (timeDistortion.forward)
            //        {
            //            orbitTimeDisplacement += timeDistortion.accelerated ? orbitForwardAcceleratedFactor * Time.deltaTime : orbitForwardFactor * Time.deltaTime;
            //        }
            //        else
            //        {
            //            orbitTimeDisplacement -= timeDistortion.accelerated ? orbitBackwardAcceleratedFactor * Time.deltaTime : orbitBackwardFactor * Time.deltaTime;
            //        }
            //        OrbitTimeUpdate?.Invoke(gameTime + orbitTimeDisplacement);
            //    break;
            //}
            UpdatePowerups(Time.deltaTime);
        }
        else
        {
            gameTime += Time.deltaTime;
            maxNaturalTime = Mathf.Max(maxNaturalTime, gameTime);
            GameTimeUpdate?.Invoke(gameTime);
        }
    }

    void UpdatePowerups(float time)
    {
        List<ulong> distortionExpirationIds = new List<ulong>();
        foreach (ulong clientId in playerTimeDistortions.Keys)
        {
            Powerup p = selectedPowerups[clientId];
            float factor = playerTimeDistortions[clientId].accelerated ? acceleratedUnit : normalUnit;
            if (p is global::TimeDistortion)
            {
                global::TimeDistortion distortion = (global::TimeDistortion)p;
                factor *= timeFactor;
                distortion.timeLeft = Mathf.Max(0, distortion.timeLeft - (factor * time));
                UpdateBattery(clientId, distortion.timeLeft, distortion.maxTimeLeft);
                if (distortion.timeLeft <= 0)
                {
                    RemovePowerup(clientId, playerPowerups[clientId].IndexOf(distortion));
                    distortionExpirationIds.Add(clientId);
                }
            }
            else
            {
                OrbitShift distortion = (OrbitShift)p;
                factor *= orbitFactor;
                distortion.timeLeft = Mathf.Max(0, distortion.timeLeft - (factor * time));
                UpdateBattery(clientId, distortion.timeLeft, distortion.maxTimeLeft);
                if (distortion.timeLeft <= 0)
                {
                    RemovePowerup(clientId, playerPowerups[clientId].IndexOf(distortion));
                    distortionExpirationIds.Add(clientId);
                }
            }
        }
        foreach (ulong clientId in distortionExpirationIds)
        {
            EndTimeDistortion(clientId);
        }
    }

    public void ServerSidePlayerSetup(PlayerBall player, ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
    }

    public void ServerSideStartGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        gameTime = 0f;
        orbitTimeDisplacement = 0f;
        gameStarted = true;
        Messenger.Instance.PlayerTurn(playerIds[0]);
    }

    public void NextPlayerTurn()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (playerIds.Count == 0)
        {
            EndGame();
            return;
        }
        playerTurn = (playerTurn + 1) % playerIds.Count;
        Messenger.Instance.PlayerTurn(playerIds[playerTurn]);
    }

    public void StartTimeDistortion(ulong clientId, TimeDistortion distortion)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (distortion.type == TimeDistortionType.Time && selectedPowerups[clientId].PowerupName == "Time Distortion" ||
            distortion.type == TimeDistortionType.Orbit && selectedPowerups[clientId].PowerupName == "Orbit Shift")
        {
            playerTimeDistortions[clientId] = distortion;
        }
    }

    public void EndTimeDistortion(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (playerTimeDistortions.ContainsKey(clientId)) playerTimeDistortions.Remove(clientId);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerSpawn.position, playerSpawnRadius);
    }

    public void RemovePlayer(ulong clientId)
    {
        bool currentPlayer = false;
        if (playerIds[playerTurn] == clientId)
        {
            currentPlayer = true;
        }
        if (playerIds.Contains(clientId))
        {

        }
        playerIds.Remove(clientId);

        // remove the player body if they were destroyed from destruction (not disconnect)
        bool playerStillConnected = false;
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (id == clientId) playerStillConnected = true;
        }
        if (playerStillConnected) RemovePlayerBody(clientId, playerBalls[clientId].transform.position);

        if (currentPlayer) NextPlayerTurn();
    }

    public void PlayerReachedGoal(PlayerBall player)
    {
        victoriousPlayers.Add(player.OwnerClientId);
        Destroy(player.gameObject);
        Messenger.Instance.PlayerVictory(player.OwnerClientId, victoriousPlayers.IndexOf(player.OwnerClientId) + 1);
    }

    public void RemovePlayerBody(ulong clientId, Vector2 position)
    {
        Messenger.Instance.RemovePlayerBody(clientId, position);
    }

    public void EndGame()
    {
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Messenger.Instance.EndGame(id, victoriousPlayers.IndexOf(id) + 1);
        }
    }

    public void PickupPowerup(ulong clientId, Powerup powerup)
    {
        List<Powerup> playerPowerupList = playerPowerups[clientId];
        if (playerPowerupList.Count == 3)
        {
            playerPowerupList.RemoveAt(0);
        }
        playerPowerupList.Add(powerup);
    }

    public void SelectPowerup(ulong clientId, int index)
    {
        List<Powerup> playerPowerupList = playerPowerups[clientId];
        Debug.Log(playerPowerupList.Count);
        if (index < playerPowerupList.Count)
        {
            selectedPowerups[clientId] = playerPowerupList[index];
            Powerup powerup = selectedPowerups[clientId];
            Messenger.Instance.UpdateSelectedPowerup(clientId, powerup.PowerupName);
            if (powerup is global::TimeDistortion)
            {
                global::TimeDistortion timeDistortion = (global::TimeDistortion)powerup;
                UpdateBattery(clientId, timeDistortion.timeLeft, timeDistortion.maxTimeLeft);
            }
            else if (powerup is OrbitShift)
            {
                OrbitShift orbitShift = (OrbitShift)powerup;
                UpdateBattery(clientId, orbitShift.timeLeft, orbitShift.maxTimeLeft);
            }
        }
    }

    void UpdateBattery(ulong clientId, float time, float maxTime)
    {
        Messenger.Instance.UpdateBattery(clientId, Mathf.RoundToInt(time * 100 / maxTime));
    }

    public bool ConsumeExtraLifeIfPossible(ulong clientId)
    {
        List<Powerup> playerPowerupList = playerPowerups[clientId];
        int index = -1;
        for (int i = 0; i < playerPowerupList.Count; i++)
        {
            if (playerPowerupList[i] is ExtraLife)
            {
                index = i;
                break;
            }
        }
        if (index < 0) return false;
        RemovePowerup(clientId, index);
        return true;
    }

    void RemovePowerup(ulong clientId, int index)
    {
        List<Powerup> playerPowerupList = playerPowerups[clientId];
        Powerup powerup = playerPowerupList[index];
        playerPowerupList.RemoveAt(index);
        if (selectedPowerups[clientId] == powerup)
        {
            selectedPowerups[clientId] = null;
            Messenger.Instance.UpdateSelectedPowerup(clientId, null);
        }
        Messenger.Instance.RemovePowerup(clientId, index);
    }
}
