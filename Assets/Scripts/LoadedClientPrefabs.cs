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

    [Tooltip("Supernova projection")]
    [SerializeField] private GameObject supernovaProjection;
    public GameObject SupernovaProjection => supernovaProjection;

    [Tooltip("Star orbit line prefab")]
    [SerializeField] private GameObject starOrbitLine;
    public GameObject StarOrbitLine => starOrbitLine;

    [Tooltip("Player soul prefab")]
    [SerializeField] private GameObject soul;
    public GameObject Soul => soul;

    [Tooltip("Follow text")]
    [SerializeField] private GameObject followText;
    public GameObject FollowText => followText;

    [Tooltip("Warning")]
    [SerializeField] private GameObject warning;
    public GameObject Warning => warning;
}
