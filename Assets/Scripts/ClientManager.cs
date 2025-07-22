using System.Collections;
using System.Timers;
using Unity.Netcode;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
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
            if (Vector2.Distance(mainCamera.transform.position, player.transform.position) < 0.1f)
            {
                cameraRoot.position = player.transform.position;
            }
            else
            {
                cameraRoot.position = Vector3.Lerp(cameraRoot.position, player.transform.position, 2f * Time.deltaTime);
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
}
