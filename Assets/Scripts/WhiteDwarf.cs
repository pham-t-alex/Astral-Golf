using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Star;

public class WhiteDwarf : OrbitingObject
{
    private float baseAge;
    private float age = 0;
    private bool destroyed = false;
    [SerializeField] private Nebula nebula;
    float nebulaScale;

    public void InitializeWhiteDwarf(float baseAge, float age, float nebulaScale, float nebulaAccel)
    {
        this.baseAge = baseAge;
        this.age = age;
        this.nebulaScale = nebulaScale;
        nebula.transform.localScale = new Vector3(nebulaScale / transform.localScale.x, nebulaScale / transform.localScale.y, 1f);
        nebula.InitializeNebula(nebulaAccel);

        Manager.Instance.GameTimeUpdate += UpdateWhiteDwarfAge;
    }

    protected override void StartServerSetup()
    {
        base.StartServerSetup();
        InitializeNebulaClientRpc(nebulaScale, default);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void InitializeNebulaClientRpc(float nebulaScale, RpcParams rpcParams)
    {
        nebula.transform.localScale = new Vector3(nebulaScale / transform.localScale.x, nebulaScale / transform.localScale.y, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer || collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        // handle power up collection logic
        PowerupData powerupData = Manager.Instance.Powerups[Random.Range(0, Manager.Instance.Powerups.Count)];
        Powerup powerup = powerupData.GetPowerup();
        collision.GetComponent<PlayerBall>().PickupPowerup(powerup);
        Destroy(gameObject);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!IsServer) return;
        Destroy(nebula.gameObject);
    }

    public void UpdateWhiteDwarfAge(float time)
    {
        if (!IsServer) return;
        if (destroyed) return;
        age = baseAge + time;
        if (age < 0)
        {
            destroyed = true;
            float newAge = age + Manager.Instance.RedGiantMaxAge;
            float newBaseAge = newAge - time;
            GameObject newStar = Instantiate(Manager.Instance.LoadedPrefabs.RedGiantStar, transform.position, Quaternion.identity);
            Star starComp = newStar.GetComponent<Star>();
            starComp.InitializeCelestialObject(scale);
            starComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            starComp.InitializeStar(newBaseAge, newAge, StarType.RedGiant, StarFate.Nebula);
            newStar.GetComponent<NetworkObject>().Spawn();
            Destroy(gameObject);
            return;
        }
    }
}
