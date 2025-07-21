using UnityEngine;

[CreateAssetMenu(fileName = "LoadedPrefabs", menuName = "Scriptable Objects/LoadedPrefabs")]
public class LoadedPrefabs : ScriptableObject
{
    [Tooltip("Prefab for the player ball")]
    [SerializeField] private GameObject playerBall;
    public GameObject PlayerBall => playerBall;

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
}
