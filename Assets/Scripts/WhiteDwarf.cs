using System.Collections.Generic;
using UnityEngine;
using static Star;

public class WhiteDwarf : OrbitingObject
{
    private float baseAge;
    private float age = 0;
    private bool destroyed = false;
    [SerializeField] private Nebula nebula;

    public void InitializeWhiteDwarf(float baseAge, float age, float nebulaScale, float nebulaAccel)
    {
        this.baseAge = baseAge;
        this.age = age;
        nebula.transform.localScale = new Vector3(nebulaScale / transform.localScale.x, nebulaScale / transform.localScale.y, 1f);
        nebula.InitializeNebula(nebulaAccel);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        // handle power up collection logic
        Destroy(gameObject);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(nebula.gameObject);
    }

    protected override void StartSetup()
    {
        base.StartSetup();
        Manager.Instance.GameTimeUpdate += UpdateWhiteDwarfAge;
    }

    public void UpdateWhiteDwarfAge(float time)
    {
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
            Destroy(gameObject);
            return;
        }
    }
}
