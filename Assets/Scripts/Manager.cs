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

    [Tooltip("Camera")]
    [SerializeField] private Camera mainCamera;

    private float gameTime = 0f;
    public float GameTime => gameTime;
    public event Action<float> GameTimeUpdate;

    private float orbitTimeDisplacement = 0f;
    public float OrbitTimeDisplacement => orbitTimeDisplacement;
    public float OrbitTime => gameTime + orbitTimeDisplacement;
    // should trigger on game time updates and orbit time displacement changes
    public event Action<float> OrbitTimeUpdate;

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

    private void Awake()
    {
        instance = this;
        GameTimeUpdate += (time) => OrbitTimeUpdate?.Invoke(time + orbitTimeDisplacement);
    }

    private void Update()
    {
        if (distortionActive)
        {
            switch (timeDistortion.type)
            {
                case TimeDistortionType.Time:
                    if (timeDistortion.forward)
                    {
                        gameTime += timeDistortion.accelerated ? timeForwardAcceleratedFactor * Time.deltaTime : timeForwardFactor * Time.deltaTime;
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
                        orbitTimeDisplacement += timeDistortion.accelerated ? timeForwardAcceleratedFactor * Time.deltaTime : timeForwardFactor * Time.deltaTime;
                    }
                    else
                    {
                        orbitTimeDisplacement -= timeDistortion.accelerated ? timeBackwardAcceleratedFactor * Time.deltaTime : timeBackwardFactor * Time.deltaTime;
                    }
                    OrbitTimeUpdate?.Invoke(gameTime + orbitTimeDisplacement);
                break;
            }
        }
        else
        {
            gameTime += Time.deltaTime;
            GameTimeUpdate?.Invoke(gameTime);
        }
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

    public void StartTimeDistortion(TimeDistortion distortion)
    {
        timeDistortion = distortion;
        distortionActive = true;
    }

    public void StopTimeDistortion()
    {
        distortionActive = false;
    }
}
