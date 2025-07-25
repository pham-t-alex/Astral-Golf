using Unity.Netcode;
using UnityEngine;

public class BlackHoleSpawn : OrbitingObjectSpawn
{
    [Header("Black Hole Settings")]
    [SerializeField] private float pullForceFactor = 1f;
    [SerializeField] private float pullScale = 5f;
    protected override void DrawObject(Vector2 position)
    {
        Gizmos.color = objColor;
        Gizmos.DrawSphere(position, 5.4f * scale);
        Gizmos.DrawWireSphere(position, 25 * pullScale);
    }

    public override void SpawnObject()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        GameObject blackHole = Instantiate(Manager.Instance.LoadedPrefabs.BlackHole, ObjPosition, Quaternion.identity);
        BlackHole blackHoleComp = blackHole.GetComponent<BlackHole>();
        blackHoleComp.InitializeCelestialObject(scale);
        blackHoleComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
        blackHoleComp.InitializeBlackHole(0, 0, pullForceFactor);
        blackHole.GetComponent<NetworkObject>().Spawn();
    }
}
