using Unity.VisualScripting;
using UnityEngine;

public class Star : OrbitingObject
{
    public enum StarFate
    {
        Nebula,
        NeutronStar,
        BlackHole
    }

    public enum StarType
    {
        MainSequence,
        RedGiant
    }

    private float baseAge;
    private float age = 0;
    private StarType type;
    private StarFate fate;
    private bool destroyed = false;

    private GameObject redGiantProjection;
    private GameObject deathProjection;

    public void InitializeStar(float baseAge, float age, StarType type, StarFate fate)
    {
        this.baseAge = baseAge;
        this.age = age;
        this.type = type;
        this.fate = fate;
    }

    protected override void StartSetup()
    {
        base.StartSetup();
        Manager.Instance.GameTimeUpdate += UpdateStarAge;
    }

    public void UpdateStarAge(float time)
    {
        if (destroyed) return;
        age = baseAge + time;
        float newAge;
        float newBaseAge;
        if (type == StarType.MainSequence && age > Manager.Instance.StarMaxAge)
        {
            destroyed = true;
            newAge = age - Manager.Instance.StarMaxAge;
            newBaseAge = newAge - time;
            GameObject newStar = Instantiate(Manager.Instance.LoadedPrefabs.RedGiantStar, transform.position, Quaternion.identity);
            Star starComp = newStar.GetComponent<Star>();
            starComp.InitializeCelestialObject(scale);
            starComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            starComp.InitializeStar(newBaseAge, newAge, StarType.RedGiant, fate);
            Destroy(gameObject);
            return;
        }
        if (type == StarType.MainSequence) return;
        // red giant
        if (age < 0)
        {
            destroyed = true;
            newAge = age + Manager.Instance.StarMaxAge;
            newBaseAge = newAge - time;
            GameObject newStar = Instantiate(Manager.Instance.LoadedPrefabs.Star, transform.position, Quaternion.identity);
            Star starComp = newStar.GetComponent<Star>();
            starComp.InitializeCelestialObject(scale);
            starComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            starComp.InitializeStar(newBaseAge, newAge, StarType.MainSequence, fate);
            Destroy(gameObject);
            return;
        }
        if (age <= Manager.Instance.RedGiantMaxAge) return;
        // red giant dies
        destroyed = true;
        newAge = age - Manager.Instance.RedGiantMaxAge;
        newBaseAge = newAge - time;
        if (fate == StarFate.Nebula)
        {
            GameObject whiteDwarf = Instantiate(Manager.Instance.LoadedPrefabs.WhiteDwarf, transform.position, Quaternion.identity);
            WhiteDwarf whiteDwarfComp = whiteDwarf.GetComponent<WhiteDwarf>();
            whiteDwarfComp.InitializeCelestialObject(scale);
            whiteDwarfComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            whiteDwarfComp.InitializeWhiteDwarf(newBaseAge, newAge, scale * Random.Range(4, 6), Random.Range(-20, 20));
            
            Destroy(gameObject);
            return;
        }
        // supernova
        GameObject nova = Instantiate(ClientManager.Instance.LoadedPrefabs.SupernovaEffect, transform.position, Quaternion.identity);
        nova.transform.localScale = Vector3.one * scale;
        Destroy(nova, 5f);

        float maxDistance = 10f * scale; // max distance for supernova effect
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxDistance, LayerMask.GetMask("Player"));
        foreach (Collider2D collider in colliders)
        {
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            // linear dropoff (instead of inverse distance squared) so it's not as extreme
            rb.AddForce(Manager.Instance.SupernovaForce * scale * (1 - (Vector2.Distance(transform.position, collider.transform.position) / maxDistance)) * (collider.transform.position - transform.position).normalized, ForceMode2D.Impulse);
        }
        if (fate == StarFate.NeutronStar)
        {
            GameObject neutronStar = Instantiate(Manager.Instance.LoadedPrefabs.NeutronStar, transform.position, Quaternion.identity);
            NeutronStar neutronStarComp = neutronStar.GetComponent<NeutronStar>();
            neutronStarComp.InitializeCelestialObject(scale);
            neutronStarComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            neutronStarComp.InitializeNeutronStar(newBaseAge, newAge, scale * Random.Range(15, 25), scale * Random.Range(4, 6));
        }
        else
        {
            GameObject blackHole = Instantiate(Manager.Instance.LoadedPrefabs.BlackHole, transform.position, Quaternion.identity);
            BlackHole blackHoleComp = blackHole.GetComponent<BlackHole>();
            blackHoleComp.InitializeCelestialObject(scale);
            blackHoleComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            blackHoleComp.InitializeBlackHole(newBaseAge, newAge, scale * Random.Range(15, 25), scale * Random.Range(4, 6));
        }
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        EvolutionProjection();
    }

    void EvolutionProjection()
    {
        if (type == StarType.MainSequence)
        {
            if (age >= Manager.Instance.StarMaxAge - Manager.Instance.ProjectionTime)
            {
                Vector2 redGiantPosition = EllipsePosition(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * (startingAngle + (Manager.Instance.OrbitTime + Manager.Instance.StarMaxAge - age) * angularVelocity));
                if (redGiantProjection == null)
                {
                    redGiantProjection = Instantiate(ClientManager.Instance.LoadedPrefabs.RedGiantProjection, redGiantPosition, Quaternion.identity);
                }
                redGiantProjection.transform.position = redGiantPosition;
            }
            else if (age < Manager.Instance.StarMaxAge - Manager.Instance.ProjectionTime && redGiantProjection != null)
            {
                Destroy(redGiantProjection);
                redGiantProjection = null;
            }
            return;
        }
        // red giant
        if (age >= Manager.Instance.RedGiantMaxAge - Manager.Instance.ProjectionTime)
        {
            Vector2 deathPosition = EllipsePosition(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * (startingAngle + (Manager.Instance.OrbitTime + Manager.Instance.RedGiantMaxAge - age) * angularVelocity));
            if (deathProjection == null)
            {
                switch (fate)
                {
                    case StarFate.Nebula:
                        deathProjection = Instantiate(ClientManager.Instance.LoadedPrefabs.WhiteDwarfProjection, deathPosition, Quaternion.identity);
                        break;
                    case StarFate.NeutronStar:
                        deathProjection = Instantiate(ClientManager.Instance.LoadedPrefabs.NeutronStarProjection, deathPosition, Quaternion.identity);
                        break;
                    case StarFate.BlackHole:
                        deathProjection = Instantiate(ClientManager.Instance.LoadedPrefabs.BlackHoleProjection, deathPosition, Quaternion.identity);
                        break;
                }
            }
            deathProjection.transform.position = deathPosition;
        }
        else if (age < Manager.Instance.RedGiantMaxAge - Manager.Instance.ProjectionTime && deathProjection != null)
        {
            Destroy(deathProjection);
            deathProjection = null;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (redGiantProjection != null)
        {
            Destroy(redGiantProjection);
        }
        if (deathProjection != null)
        {
            Destroy(deathProjection);
        }
    }
}
