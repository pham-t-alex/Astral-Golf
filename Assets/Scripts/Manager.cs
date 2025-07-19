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
}
