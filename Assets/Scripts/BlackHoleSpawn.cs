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
        Gizmos.DrawSphere(position, 0.5f * scale);
        Gizmos.DrawWireSphere(position, 0.5f * pullScale);
    }

    public override void SpawnObject()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        GameObject blackHole = Instantiate(Manager.Instance.LoadedPrefabs.BlackHole, ObjPosition, Quaternion.identity);
        BlackHole blackHoleComp = blackHole.GetComponent<BlackHole>();
        blackHoleComp.InitializeCelestialObject(scale);
        blackHoleComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
        blackHoleComp.InitializeBlackHole(0, 0, pullForceFactor, pullScale);
        blackHole.GetComponent<NetworkObject>().Spawn();
    }
}
