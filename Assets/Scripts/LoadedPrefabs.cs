using UnityEngine;

[CreateAssetMenu(fileName = "LoadedPrefabs", menuName = "Scriptable Objects/LoadedPrefabs")]
public class LoadedPrefabs : ScriptableObject
{
    [Tooltip("Prefab for player launch line")]
    [SerializeField] private GameObject shootLine;
    public GameObject ShootLine => shootLine;
}
