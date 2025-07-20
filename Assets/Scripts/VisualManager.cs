using UnityEngine;

public class VisualManager : MonoBehaviour
{
    private static VisualManager instance;
    public static VisualManager Instance => instance;
    
    [Header("Visual Settings")]
    [Tooltip("Number of segments to represent a star's orbit")]
    [SerializeField] private int orbitSegments = 100;
    public int OrbitSegments => orbitSegments;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
