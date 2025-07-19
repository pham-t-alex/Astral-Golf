using UnityEngine;

public class Manager : MonoBehaviour
{
    [Tooltip("Prefabs")]
    [SerializeField] private LoadedPrefabs loadedPrefabs;
    public LoadedPrefabs LoadedPrefabs => loadedPrefabs;
    private static Manager instance;
    public static Manager Instance => instance;

    private void Awake()
    {
        instance = this;
    }
}
