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

    [Tooltip("Red giant prefab")]
    [SerializeField] private GameObject redGiantStar;
    public GameObject RedGiantStar => redGiantStar;

    [Tooltip("White dwarf prefab")]
    [SerializeField] private GameObject whiteDwarf;
    public GameObject WhiteDwarf => whiteDwarf;

    [Tooltip("Neutron Star prefab")]
    [SerializeField] private GameObject neutronStar;
    public GameObject NeutronStar => neutronStar;

    [Tooltip("Black Hole prefab")]
    [SerializeField] private GameObject blackHole;
    public GameObject BlackHole => blackHole;

    [Tooltip("Supernova effect prefab")]
    [SerializeField] private GameObject supernovaEffect;
    public GameObject SupernovaEffect => supernovaEffect;

    [Tooltip("Red giant projection")]
    [SerializeField] private GameObject redGiantProjection;
    public GameObject RedGiantProjection => redGiantProjection;

    [Tooltip("White dwarf projection")]
    [SerializeField] private GameObject whiteDwarfProjection;
    public GameObject WhiteDwarfProjection => whiteDwarfProjection;

    [Tooltip("Neutron Star projection")]
    [SerializeField] private GameObject neutronStarProjection;
    public GameObject NeutronStarProjection => neutronStarProjection;

    [Tooltip("Black Hole projection")]
    [SerializeField] private GameObject blackHoleProjection;
    public GameObject BlackHoleProjection => blackHoleProjection;

    [Tooltip("Star orbit line prefab")]
    [SerializeField] private GameObject starOrbitLine;
    public GameObject StarOrbitLine => starOrbitLine;
}
