using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientManager : MonoBehaviour
{
    // Server-side state: track selected powerup index per player
    private Dictionary<ulong, int> playerSelectedPowerup = new Dictionary<ulong, int>();
    private static ClientManager instance;
    public static ClientManager Instance => instance;

    [Header("Visual Settings")]
    [SerializeField] private Transform orbitParent;
    public Transform OrbitParent => orbitParent;
    [Tooltip("Number of segments to represent a star's orbit")]
    [SerializeField] private int orbitSegments = 100;
    public int OrbitSegments => orbitSegments;

    [Tooltip("Gap between orbit ellipses")]
    [SerializeField] private float orbitGap = 1f;
    public float OrbitGap => orbitGap;

    [Tooltip("Max orbit axes length")]
    [SerializeField] private float maxOrbitAxes = 20f;
    public float MaxOrbitAxes => maxOrbitAxes;

    [Tooltip("Length of screen shake")]
    [SerializeField] private float screenShakeDuration = 1f;

    [Tooltip("Screen shake magnitude factor")]
    [SerializeField] private float screenShakeFactor = 1f;

    [Header("Other")]
    [Tooltip("Client Prefabs")]
    [SerializeField] private LoadedClientPrefabs loadedPrefabs;
    public LoadedClientPrefabs LoadedPrefabs => loadedPrefabs;
    [Tooltip("Client Sprites")]
    [SerializeField] private LoadedSprites loadedSprites;
    public LoadedSprites LoadedSprites => loadedSprites;

    [Tooltip("Camera Root")]
    [SerializeField] private Transform cameraRoot;
    [Tooltip("Camera")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Powerups")]
    [SerializeField] private List<PowerupData> powerups = new List<PowerupData>();
    public List<PowerupData> Powerups => powerups;

    private PlayerBall player;
    public PlayerBall PlayerBall => player;
    private PlayerSoul soul;
    public PlayerSoul Soul => soul;

    private bool currentTurn = false;
    public bool CurrentTurn => currentTurn;

    private bool astralProjecting = false;
    public bool AstralProjecting => astralProjecting;

    private void Awake()
    {
        if (!NetworkManager.Singleton.IsClient)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void LateUpdate()
    {
        Transform target = astralProjecting ? soul.transform : player.transform;
        if (mainCamera != null && target != null)
        {
            if (Vector2.Distance(mainCamera.transform.position, target.position) < 0.1f)
            {
                cameraRoot.position = target.position;
            }
            else
            {
                cameraRoot.position = Vector3.Lerp(cameraRoot.position, target.position, 2f * Time.deltaTime);
            }
        }
    }
    
    public void SetPlayer(PlayerBall playerBall)
    {
        player = playerBall;
        if (mainCamera != null)
        {
            cameraRoot.position = player.transform.position;
        }
    }

    public void StartTurn()
    {
        currentTurn = true;
        UIManager.Instance.SetTurnIndicatorActive();
    }

    public void PlayerLaunch()
    {
        currentTurn = false;
        UIManager.Instance.SetTurnIndicatorInactive();
    }

    public void TriggerScreenShake(float strength)
    {
        StartCoroutine(ScreenShake(strength));
    }

    private IEnumerator ScreenShake(float strength)
    {
        float time = 0f;
        float adjustedStrength = screenShakeFactor * strength;
        float duration = screenShakeDuration;

        while (time < screenShakeDuration)
        {
            // screen shake fade
            float factor = Mathf.Lerp(1, 0, time / screenShakeDuration);
            mainCamera.transform.localPosition = new Vector3(factor * Random.Range(-adjustedStrength, adjustedStrength),
                factor * Random.Range(-adjustedStrength, adjustedStrength), mainCamera.transform.localPosition.z);

            time += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = new Vector3(0, 0, mainCamera.transform.localPosition.z);
    }

    public void HandleAstralProject()
    {
        if (player == null) return;
        astralProjecting = !astralProjecting;
        if (astralProjecting)
        {
            player.GetComponent<PlayerInput>().enabled = false;
            soul = Instantiate(loadedPrefabs.Soul, player.transform.position, Quaternion.identity).GetComponent<PlayerSoul>();
        }
        else
        {
            soul.GetComponent<PlayerInput>().enabled = false;
            Destroy(soul.gameObject);
            player.GetComponent<PlayerInput>().enabled = true;
        }
    }

    // --- Powerup Selection Networking ---
    // Called by UIManager to select a powerup slot
    public void SendSelectPowerupRPC(int index)
    {
        SelectPowerupServerRpc(index);
    }

    // Called by UIManager to use time distortion
    public void SendTimeDistortionRPC(bool accelerated)
    {
        TimeDistortionServerRpc(accelerated);
    }

    // Called by UIManager to deselect powerup
    public void SendDeselectPowerupRPC()
    {
        DeselectPowerupServerRpc();
    }

    // ServerRpc: receives selection from client, updates server state, then notifies client
    [ServerRpc]
    private void SelectPowerupServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        // Validate index (for demo, assume valid if >=0)
        playerSelectedPowerup[clientId] = index;
        SelectedPowerupClientRpc(index, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
    }

    [ServerRpc]
    private void DeselectPowerupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        playerSelectedPowerup.Remove(clientId);
        DeselectedPowerupClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
    }
    // ServerRpc for time distortion
    [ServerRpc]
    private void TimeDistortionServerRpc(bool accelerated, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        int selectedIndex = playerSelectedPowerup.ContainsKey(clientId) ? playerSelectedPowerup[clientId] : -1;
        // TODO: Apply time distortion logic based on selectedIndex and accelerated
        // For now, just echo back to client (could trigger effects, etc.)
        // You can add a ClientRpc here to notify the client of the result
    }
    // ClientRpc to update powerup list when a powerup is consumed/expired
    [ClientRpc]
    private void RemovePowerupClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        // Remove powerup at index and update UI
        if (index >= 0 && index < UIManager.Instance.Powerups.Count)
        {
            UIManager.Instance.Powerups.RemoveAt(index);
            UIManager.Instance.PickupPowerup(null); // Force UI update
        }
        // Optionally, deselect if the removed powerup was selected
        if (UIManager.Instance.SelectedPowerupIndex == index)
        {
            UIManager.Instance.OnServerDeselectedPowerup();
        }
    }

    // ClientRpc: called by server to update UI on client
    [ClientRpc]
    private void SelectedPowerupClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        UIManager.Instance.OnServerSelectedPowerup(index);
    }

    [ClientRpc]
    private void DeselectedPowerupClientRpc(ClientRpcParams clientRpcParams = default)
    {
        UIManager.Instance.OnServerDeselectedPowerup();
    }
}
