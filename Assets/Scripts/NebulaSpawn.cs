using UnityEngine;

public class NebulaSpawn : OrbitingObjectSpawn
{
    [Header("Nebula Settings")]
    [SerializeField] private float accelFactor = 0.1f;
    protected override void DrawObject(Vector2 position)
    {
        Gizmos.color = objColor;
        Gizmos.DrawWireSphere(position, 0.5f * scale);
    }

    public override void SpawnObject()
    {
        GameObject nebula = Instantiate(Manager.Instance.LoadedPrefabs.Nebula, ObjPosition, Quaternion.identity);
        Nebula nebulaComp = nebula.GetComponent<Nebula>();
        nebulaComp.InitializeCelestialObject(scale);
        nebulaComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
        nebulaComp.InitializeNebula(accelFactor);
    }
}
