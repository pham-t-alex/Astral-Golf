using UnityEngine;

public abstract class CelestialObjectSpawn : MonoBehaviour
{
    [Header("Celestial Object Settings")]
    [Tooltip("The object's scale")]
    [SerializeField] protected float scale = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnObject();
    }

    public abstract void SpawnObject();
}
