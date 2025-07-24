using UnityEngine;

[CreateAssetMenu(fileName = "LoadedClientPrefabs", menuName = "Scriptable Objects/LoadedClientPrefabs")]
public class LoadedClientPrefabs : ScriptableObject
{
    [Tooltip("Prefab for player launch line")]
    [SerializeField] private GameObject shootLine;
    public GameObject ShootLine => shootLine;

    [Tooltip("Supernova effect prefab")]
    [SerializeField] private GameObject supernovaEffect;
    public GameObject SupernovaEffect => supernovaEffect;

    [Tooltip("Supernova shockwave prefab")]
    [SerializeField] private GameObject supernovaShockwave;
    public GameObject SupernovaShockwave => supernovaShockwave;

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

    [Tooltip("Player soul prefab")]
    [SerializeField] private GameObject soul;
    public GameObject Soul => soul;
}
