using UnityEngine;

[CreateAssetMenu(fileName = "LoadedPrefabs", menuName = "Scriptable Objects/LoadedPrefabs")]
public class LoadedPrefabs : ScriptableObject
{
    [Tooltip("Prefab for player launch line")]
    [SerializeField] private GameObject shootLine;
    public GameObject ShootLine => shootLine;

    [Tooltip("Star prefab")]
    [SerializeField] private GameObject star;
    public GameObject Star => star;

    [Tooltip("Nebula prefab")]
    [SerializeField] private GameObject nebula;
    public GameObject Nebula => nebula;

    [Tooltip("Neutron Star prefab")]
    [SerializeField] private GameObject neutronStar;
    public GameObject NeutronStar => neutronStar;

    [Tooltip("Black Hole prefab")]
    [SerializeField] private GameObject blackHole;
    public GameObject BlackHole => blackHole;

    [Tooltip("Wormhole prefab")]
    [SerializeField] private GameObject wormhole;
    public GameObject Wormhole => wormhole;
}
