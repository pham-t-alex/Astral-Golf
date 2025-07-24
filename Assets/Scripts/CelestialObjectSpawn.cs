using Unity.Netcode;
using UnityEngine;

public abstract class CelestialObjectSpawn : MonoBehaviour
{
    [Header("Celestial Object Settings")]
    [Tooltip("The object's scale")]
    [SerializeField] protected float scale = 1f;
    [Tooltip("The object's color (just for level setup)")]
    [SerializeField] protected Color objColor = Color.white;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        SpawnObject();
    }

    public abstract void SpawnObject();

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        DrawObject(transform.position);
    }

    protected virtual void DrawObject(Vector2 position)
    {
        Gizmos.color = objColor;
        Gizmos.DrawSphere(position, 0.5f * scale);
    }
}
