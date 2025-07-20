using UnityEngine;

public class BlackHoleSpawn : OrbitingObjectSpawn
{
    [Header("Black Hole Settings")]
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
        GameObject blackHole = Instantiate(Manager.Instance.LoadedPrefabs.BlackHole, ObjPosition, Quaternion.identity);
        BlackHole blackHoleComp = blackHole.GetComponent<BlackHole>();
        blackHoleComp.InitializeCelestialObject(scale);
        blackHoleComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
        blackHoleComp.InitializeBlackHole(pullForceFactor, maxPullRadius);
    }
}
