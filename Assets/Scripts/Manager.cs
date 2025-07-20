using UnityEngine;
using System;

public class Manager : MonoBehaviour
{
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

    [Header("Other")]
    [Tooltip("Prefabs")]
    [SerializeField] private LoadedPrefabs loadedPrefabs;
    public LoadedPrefabs LoadedPrefabs => loadedPrefabs;
    private static Manager instance;
    public static Manager Instance => instance;

    [Tooltip("Camera")]
    [SerializeField] private Camera mainCamera;

    private float gameTime = 0f;
    public float GameTime => gameTime;
    public event Action<float> GameTimeUpdate;

    private float orbitTime = 0f;
    public float OrbitTime => orbitTime;
    // should trigger on game time updates and orbit time displacement changes
    public event Action<float> OrbitTimeUpdate;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        gameTime += Time.deltaTime;
        GameTimeUpdate?.Invoke(gameTime);
        orbitTime += Time.deltaTime;
        OrbitTimeUpdate?.Invoke(orbitTime);
    }

    private void LateUpdate()
    {
        PlayerBall player = FindFirstObjectByType<PlayerBall>();
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
}
