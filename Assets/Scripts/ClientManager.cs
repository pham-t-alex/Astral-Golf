using Unity.Netcode;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    private static ClientManager instance;
    public static ClientManager Instance => instance;
    
    [Header("Visual Settings")]
    [Tooltip("Number of segments to represent a star's orbit")]
    [SerializeField] private int orbitSegments = 100;
    public int OrbitSegments => orbitSegments;

    [Header("Other")]
    [Tooltip("Client Prefabs")]
    [SerializeField] private LoadedClientPrefabs loadedPrefabs;
    public LoadedClientPrefabs LoadedPrefabs => loadedPrefabs;
    [Tooltip("Client Sprites")]
    [SerializeField] private LoadedSprites loadedSprites;
    public LoadedSprites LoadedSprites => loadedSprites;

    [Tooltip("Camera")]
    [SerializeField] private Camera mainCamera;

    private PlayerBall player;
    private bool currentTurn = false;
    public bool CurrentTurn => currentTurn;

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
        if (mainCamera != null && player != null)
        {
            float z = mainCamera.transform.position.z;
            if (Vector2.Distance(mainCamera.transform.position, player.transform.position) < 0.1f)
            {
                mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, z);
            }
            else
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.transform.position.x, player.transform.position.y, z), 2f * Time.deltaTime);
            }
        }
    }
    
    public void SetPlayer(PlayerBall playerBall)
    {
        player = playerBall;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, mainCamera.transform.position.z);
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
}
