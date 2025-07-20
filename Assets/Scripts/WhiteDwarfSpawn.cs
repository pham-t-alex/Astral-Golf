using UnityEngine;

public class WhiteDwarfSpawn : OrbitingObjectSpawn
{
    [Header("Nebula Settings")]
    [SerializeField] private float nebulaScale = 1f;
    [SerializeField] private float accelFactor = 0.1f;
    protected override void DrawObject(Vector2 position)
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(position, 0.5f * scale);
        Gizmos.color = objColor;
        Gizmos.DrawWireSphere(position, 0.5f * nebulaScale);
    }

    public override void SpawnObject()
    {
        GameObject whiteDwarf = Instantiate(Manager.Instance.LoadedPrefabs.WhiteDwarf, ObjPosition, Quaternion.identity);
        WhiteDwarf whiteDwarfComp = whiteDwarf.GetComponent<WhiteDwarf>();
        whiteDwarfComp.InitializeCelestialObject(scale);
        whiteDwarfComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
        whiteDwarfComp.InitializeWhiteDwarf(0, 0, nebulaScale, accelFactor);
    }
}
