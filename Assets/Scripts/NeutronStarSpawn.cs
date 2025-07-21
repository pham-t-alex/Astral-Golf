using Unity.Netcode;
using UnityEngine;

public class NeutronStarSpawn : OrbitingObjectSpawn
{
    [Header("Neutron Star Settings")]
    [SerializeField] private float pullForceFactor = 1f;
    [SerializeField] private float pullScale = 5f;
    protected override void DrawObject(Vector2 position)
    {
        Gizmos.color = objColor;
        Gizmos.DrawSphere(position, 0.5f * scale);
        Gizmos.DrawWireSphere(position, 0.5f * pullScale);
    }

    public override void SpawnObject()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        GameObject neutronStar = Instantiate(Manager.Instance.LoadedPrefabs.NeutronStar, ObjPosition, Quaternion.identity);
        NeutronStar neutronStarComp = neutronStar.GetComponent<NeutronStar>();
        neutronStarComp.InitializeCelestialObject(scale);
        neutronStarComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
        neutronStarComp.InitializeNeutronStar(0, 0, pullForceFactor, pullScale);
        neutronStar.GetComponent<NetworkObject>().Spawn();
    }
}
