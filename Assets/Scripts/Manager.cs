using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections.Generic;

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
    [SerializeField] private float timeForwardFactor = 2f;
    [SerializeField] private float timeForwardAcceleratedFactor = 5f;
    [SerializeField] private float timeBackwardFactor = 2f;
    [SerializeField] private float timeBackwardAcceleratedFactor = 5f;
    [SerializeField] private float orbitForwardFactor = 2f;
    [SerializeField] private float orbitForwardAcceleratedFactor = 5f;
    [SerializeField] private float orbitBackwardFactor = 2f;
    [SerializeField] private float orbitBackwardAcceleratedFactor = 5f;

    [Header("Other")]
    [Tooltip("Prefabs")]
    [SerializeField] private LoadedPrefabs loadedPrefabs;
    public LoadedPrefabs LoadedPrefabs => loadedPrefabs;
    private static Manager instance;
    public static Manager Instance => instance;

    [Tooltip("Player spawn")]
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private float playerSpawnRadius = 3f;

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

    public enum TimeDistortionType
    {
        Time,
        Orbit
    }
    public struct TimeDistortion
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
    }

    private bool distortionActive = false;
    private TimeDistortion timeDistortion;

    private List<ulong> playerIds = new List<ulong>();
    private Dictionary<ulong, PlayerBall> playerBalls = new Dictionary<ulong, PlayerBall>();

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
            GameObject player = Instantiate(loadedPrefabs.PlayerBall, playerSpawn.position + (Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 359)) * (UnityEngine.Random.Range(0, playerSpawnRadius) * Vector2.right)), Quaternion.identity);
            playerBalls.Add(clientId, player.GetComponent<PlayerBall>());
            ServerSidePlayerSetup(player.GetComponent<PlayerBall>(), clientId);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
        ServerSideStartGame();
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer || !gameStarted) return;
        if (distortionActive)
        {
            switch (timeDistortion.type)
            {
                case TimeDistortionType.Time:
                    if (timeDistortion.forward)
                    {
                        gameTime += timeDistortion.accelerated ? timeForwardAcceleratedFactor * Time.deltaTime : timeForwardFactor * Time.deltaTime;
                        gameTime = Mathf.Min(gameTime, maxNaturalTime);
                    }
                    else
                    {
                        gameTime -= timeDistortion.accelerated ? timeBackwardAcceleratedFactor * Time.deltaTime : timeBackwardFactor * Time.deltaTime;
                        gameTime = Mathf.Max(gameTime, 0f);
                    }
                    GameTimeUpdate?.Invoke(gameTime);
                    break;
                case TimeDistortionType.Orbit:
                    if (timeDistortion.forward)
                    {
                        orbitTimeDisplacement += timeDistortion.accelerated ? orbitForwardAcceleratedFactor * Time.deltaTime : orbitForwardFactor * Time.deltaTime;
                    }
                    else
                    {
                        orbitTimeDisplacement -= timeDistortion.accelerated ? orbitBackwardAcceleratedFactor * Time.deltaTime : orbitBackwardFactor * Time.deltaTime;
                    }
                    OrbitTimeUpdate?.Invoke(gameTime + orbitTimeDisplacement);
                break;
            }
        }
        else
        {
            gameTime += Time.deltaTime;
            maxNaturalTime = Mathf.Max(maxNaturalTime, gameTime);
            GameTimeUpdate?.Invoke(gameTime);
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
        distortionActive = false;
        gameStarted = true;
        Messenger.Instance.PlayerTurn(playerIds[0]);
    }

    public void NextPlayerTurn()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        playerTurn = (playerTurn + 1) % playerIds.Count;
        Messenger.Instance.PlayerTurn(playerIds[playerTurn]);
    }

    public void StartTimeDistortion(TimeDistortion distortion)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        timeDistortion = distortion;
        distortionActive = true;
    }

    public void StopTimeDistortion()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        distortionActive = false;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerSpawn.position, playerSpawnRadius);
    }
}
