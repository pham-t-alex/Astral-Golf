using Unity.Netcode;
using UnityEngine;

public class StarSpawn : OrbitingObjectSpawn
{
    [Header("Star Settings")]
    [SerializeField] private float baseAge = 0;
    [SerializeField] private Star.StarType type = Star.StarType.MainSequence;
    [SerializeField] private Star.StarFate fate = Star.StarFate.Nebula;

    public override void SpawnObject()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        GameObject star;
        if (type == Star.StarType.MainSequence)
        {
            star = Instantiate(Manager.Instance.LoadedPrefabs.Star, ObjPosition, Quaternion.identity);
        }
        else
        {
            star = Instantiate(Manager.Instance.LoadedPrefabs.RedGiantStar, ObjPosition, Quaternion.identity);
        }
        Star starComp = star.GetComponent<Star>();
        starComp.InitializeCelestialObject(scale);
        starComp.InitializeOrbit(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
        starComp.InitializeStar(baseAge, baseAge, type, fate);
        star.GetComponent<NetworkObject>().Spawn();
    }
}
