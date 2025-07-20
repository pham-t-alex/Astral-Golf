using UnityEngine;
using System;

public class Manager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameTime = 0f;
    public float GameTime => gameTime;
    public event Action<float> GameTimeUpdate;

    [SerializeField] private float orbitTimeDisplacement = 0f;
    public float OrbitTimeDisplacement => orbitTimeDisplacement;
    public float OrbitTime => gameTime + orbitTimeDisplacement;
    // should trigger on game time updates and orbit time displacement changes
    public event Action<float> OrbitTimeUpdate;

    [Header("Other")]
    [Tooltip("Prefabs")]
    [SerializeField] private LoadedPrefabs loadedPrefabs;
    public LoadedPrefabs LoadedPrefabs => loadedPrefabs;
    private static Manager instance;
    public static Manager Instance => instance;

    [Tooltip("Camera")]
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        instance = this;
        GameTimeUpdate += (time) => OrbitTimeUpdate?.Invoke(time + orbitTimeDisplacement);
    }

    private void Update()
    {
        gameTime += Time.deltaTime;
        GameTimeUpdate?.Invoke(gameTime);
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
