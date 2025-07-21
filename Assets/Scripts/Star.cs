using Unity.Netcode;
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
    private NetworkVariable<StarType> type = new NetworkVariable<StarType>(StarType.MainSequence);
    private StarType typeInit;
    private NetworkVariable<StarFate> fate = new NetworkVariable<StarFate>(StarFate.Nebula);
    private StarFate fateInit;
    private bool destroyed = false;

    private GameObject projection;
    private struct ProjectionData : INetworkSerializable
    {
        public Vector2 position;
        public bool active;

        public ProjectionData(Vector2 position, bool active)
        {
            this.position = position;
            this.active = active;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref active);
        }
    }

    private NetworkVariable<ProjectionData> projectionData = new NetworkVariable<ProjectionData>(new ProjectionData(Vector2.zero, false));

    public void InitializeStar(float baseAge, float age, StarType type, StarFate fate)
    {
        this.baseAge = baseAge;
        this.age = age;
        typeInit = type;
        fateInit = fate;

        Manager.Instance.GameTimeUpdate += UpdateStarAge;
    }

    protected override void StartClientSetup()
    {
        base.StartClientSetup();
        projectionData.OnValueChanged += HandleProjectionDataChange;
    }

    protected override void StartServerSetup()
    {
        base.StartServerSetup();
        fate.Value = fateInit;
        type.Value = typeInit;
    }

    void HandleProjectionDataChange(ProjectionData prev, ProjectionData newData)
    {
        if (!IsClient) return;

        if (newData.active && projection == null)
        {
            if (type.Value == StarType.MainSequence)
            {
                projection = Instantiate(ClientManager.Instance.LoadedPrefabs.RedGiantProjection, newData.position, Quaternion.identity);
            }
            else
            {
                switch (fate.Value)
                {
                    case StarFate.Nebula:
                        projection = Instantiate(ClientManager.Instance.LoadedPrefabs.WhiteDwarfProjection, newData.position, Quaternion.identity);
                        break;
                    case StarFate.NeutronStar:
                        projection = Instantiate(ClientManager.Instance.LoadedPrefabs.NeutronStarProjection, newData.position, Quaternion.identity);
                        break;
                    case StarFate.BlackHole:
                        projection = Instantiate(ClientManager.Instance.LoadedPrefabs.BlackHoleProjection, newData.position, Quaternion.identity);
                        break;
                }
            }
        }
        else if (!newData.active && projection != null)
        {
            Destroy(projection);
        }
        projection.transform.position = newData.position;
    }

    public void UpdateStarAge(float time)
    {
        if (!IsServer) return;
        if (destroyed) return;
        age = baseAge + time;
        float newAge;
        float newBaseAge;

        // main sequence -> red giant
        if (type.Value == StarType.MainSequence && age > Manager.Instance.StarMaxAge)
        {
            destroyed = true;
            newAge = age - Manager.Instance.StarMaxAge;
            newBaseAge = newAge - time;
            GameObject newStar = Instantiate(Manager.Instance.LoadedPrefabs.RedGiantStar, transform.position, Quaternion.identity);
            Star starComp = newStar.GetComponent<Star>();
            starComp.InitializeCelestialObject(scale);
            starComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            starComp.InitializeStar(newBaseAge, newAge, StarType.RedGiant, fate.Value);
            newStar.GetComponent<NetworkObject>().Spawn();
            Destroy(gameObject);
            return;
        }
        if (type.Value == StarType.MainSequence)
        {
            //if (age >= Manager.Instance.StarMaxAge - Manager.Instance.ProjectionTime)
            //{
            //    Vector2 projectionPosition = EllipsePosition(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * (startingAngle + (Manager.Instance.OrbitTime + Manager.Instance.StarMaxAge - age) * angularVelocity));
            //    projectionData.Value = new ProjectionData(projectionPosition, true);
            //}
            //else if (age < Manager.Instance.StarMaxAge - Manager.Instance.ProjectionTime)
            //{
            //    projectionData.Value = new ProjectionData(Vector2.zero, false);
            //}
            return;
        }
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
            starComp.InitializeStar(newBaseAge, newAge, StarType.MainSequence, fate.Value);
            newStar.GetComponent<NetworkObject>().Spawn();
            Destroy(gameObject);
            return;
        }
        if (age <= Manager.Instance.RedGiantMaxAge)
        {
            //if (age >= Manager.Instance.RedGiantMaxAge - Manager.Instance.ProjectionTime)
            //{
            //    Vector2 projectionPosition = EllipsePosition(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * (startingAngle + (Manager.Instance.OrbitTime + Manager.Instance.RedGiantMaxAge - age) * angularVelocity));
            //    projectionData.Value = new ProjectionData(projectionPosition, true);
            //}
            //else if (age < Manager.Instance.RedGiantMaxAge - Manager.Instance.ProjectionTime)
            //{
            //    projectionData.Value = new ProjectionData(Vector2.zero, false);
            //}
            return;
        }
        // red giant dies
        destroyed = true;
        newAge = age - Manager.Instance.RedGiantMaxAge;
        newBaseAge = newAge - time;
        if (fate.Value == StarFate.Nebula)
        {
            GameObject whiteDwarf = Instantiate(Manager.Instance.LoadedPrefabs.WhiteDwarf, transform.position, Quaternion.identity);
            WhiteDwarf whiteDwarfComp = whiteDwarf.GetComponent<WhiteDwarf>();
            whiteDwarfComp.InitializeCelestialObject(scale);
            whiteDwarfComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            whiteDwarfComp.InitializeWhiteDwarf(newBaseAge, newAge, scale * Random.Range(4, 6), Random.Range(-20, 20));
            whiteDwarf.GetComponent<NetworkObject>().Spawn();
            Destroy(gameObject);
            return;
        }
        // supernova
        //GameObject nova = Instantiate(ClientManager.Instance.LoadedPrefabs.SupernovaEffect, transform.position, Quaternion.identity);
        //nova.transform.localScale = Vector3.one * scale;
        //Destroy(nova, 5f);

        float maxDistance = 10f * scale; // max distance for supernova effect
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxDistance, LayerMask.GetMask("Player"));
        foreach (Collider2D collider in colliders)
        {
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            // linear dropoff (instead of inverse distance squared) so it's not as extreme
            rb.AddForce(Manager.Instance.SupernovaForce * scale * (1 - (Vector2.Distance(transform.position, collider.transform.position) / maxDistance)) * (collider.transform.position - transform.position).normalized, ForceMode2D.Impulse);
        }
        if (fate.Value == StarFate.NeutronStar)
        {
            GameObject neutronStar = Instantiate(Manager.Instance.LoadedPrefabs.NeutronStar, transform.position, Quaternion.identity);
            NeutronStar neutronStarComp = neutronStar.GetComponent<NeutronStar>();
            neutronStarComp.InitializeCelestialObject(scale);
            neutronStarComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            neutronStarComp.InitializeNeutronStar(newBaseAge, newAge, scale * Random.Range(15, 25), scale * Random.Range(4, 6));
            neutronStar.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            GameObject blackHole = Instantiate(Manager.Instance.LoadedPrefabs.BlackHole, transform.position, Quaternion.identity);
            BlackHole blackHoleComp = blackHole.GetComponent<BlackHole>();
            blackHoleComp.InitializeCelestialObject(scale);
            blackHoleComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            blackHoleComp.InitializeBlackHole(newBaseAge, newAge, scale * Random.Range(15, 25), scale * Random.Range(4, 6));
            blackHole.GetComponent<NetworkObject>().Spawn();
        }
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (!IsServer) return;
        EvolutionProjection();
    }

    void EvolutionProjection()
    {
        if (type.Value == StarType.MainSequence)
        {
            if (age >= Manager.Instance.StarMaxAge - Manager.Instance.ProjectionTime)
            {
                Vector2 projectionPosition = EllipsePosition(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * (startingAngle + (Manager.Instance.OrbitTime + Manager.Instance.StarMaxAge - age) * angularVelocity));
                projectionData.Value = new ProjectionData(projectionPosition, true);
            }
            else if (age < Manager.Instance.StarMaxAge - Manager.Instance.ProjectionTime)
            {
                projectionData.Value = new ProjectionData(Vector2.zero, false);
            }
            return;
        }
        // red giant
        if (age >= Manager.Instance.RedGiantMaxAge - Manager.Instance.ProjectionTime)
        {
            Vector2 projectionPosition = EllipsePosition(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * (startingAngle + (Manager.Instance.OrbitTime + Manager.Instance.RedGiantMaxAge - age) * angularVelocity));
            projectionData.Value = new ProjectionData(projectionPosition, true);
        }
        else if (age < Manager.Instance.RedGiantMaxAge - Manager.Instance.ProjectionTime)
        {
            projectionData.Value = new ProjectionData(Vector2.zero, false);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!IsClient) return;
        if (projection != null)
        {
            Destroy(projection);
        }
    }
}
