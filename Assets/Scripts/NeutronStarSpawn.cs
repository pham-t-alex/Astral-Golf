using UnityEngine;

public class NeutronStarSpawn : OrbitingObjectSpawn
{
    [Header("Neutron Star Settings")]
    [SerializeField] private float pullForceFactor = 1f;
    [SerializeField] private float maxPullRadius = 10f;
    protected override void DrawObject(Vector2 position)
    {
        Gizmos.color = objColor;
        Gizmos.DrawSphere(position, 0.5f * scale);
        Gizmos.DrawWireSphere(position, maxPullRadius);
    }

    public override void SpawnObject()
    {
        GameObject neutronStar = Instantiate(Manager.Instance.LoadedPrefabs.NeutronStar, ObjPosition, Quaternion.identity);
        NeutronStar neutronStarComp = neutronStar.GetComponent<NeutronStar>();
        neutronStarComp.InitializeCelestialObject(scale);
        neutronStarComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
        neutronStarComp.InitializeNeutronStar(pullForceFactor, maxPullRadius);
    }
}
