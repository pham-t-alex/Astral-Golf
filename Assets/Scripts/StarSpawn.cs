using UnityEngine;

public class StarSpawn : OrbitingObjectSpawn
{
    [Header("Star Settings")]
    [SerializeField] private float age = 0;
    public override void SpawnObject()
    {
        GameObject star = Instantiate(Manager.Instance.LoadedPrefabs.Star, ObjPosition, Quaternion.identity);
        Star starComp = star.GetComponent<Star>();
        starComp.InitializeCelestialObject(scale);
        starComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
    }
}
