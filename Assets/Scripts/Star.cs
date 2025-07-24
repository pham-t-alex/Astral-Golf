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

    // 0 to 1
    // 0 represents fully red giant, 1 represents fully hot
    private NetworkVariable<float> hotTransition = new NetworkVariable<float>(0);

    private GameObject projection;
    private GameObject warning;
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
        if (type.Value == StarType.MainSequence && fate.Value != StarFate.Nebula)
        {
            GetComponent<SpriteRenderer>().sprite = ClientManager.Instance.LoadedSprites.BlueStar;
        }
        HandleProjectionDataChange(default, projectionData.Value);
        projectionData.OnValueChanged += HandleProjectionDataChange;
        hotTransition.OnValueChanged += HandleHotTransition;
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
            projection = Instantiate(ClientManager.Instance.LoadedPrefabs.SupernovaProjection, newData.position, Quaternion.identity);
            projection.transform.position = newData.position;
            warning = Instantiate(ClientManager.Instance.LoadedPrefabs.Warning);
            warning.GetComponent<InfoFollow>().SetTarget(projection.transform);
        }
        else if (!newData.active && projection != null)
        {
            Destroy(warning);
            Destroy(projection);
        }
    }

    void HandleHotTransition(float prev, float newVal)
    {
        if (!IsClient) return;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1 - newVal);
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
            float transitionAge = Manager.Instance.RedGiantMaxAge - Manager.Instance.HotGiantTransitionGap;
            hotTransition.Value = Mathf.Clamp((age - transitionAge) / Manager.Instance.HotTransitionTime, 0, 1);
            return;
        }
        // red giant dies
        destroyed = true;
        newAge = age - Manager.Instance.RedGiantMaxAge;
        newBaseAge = newAge - time;
        if (fate.Value == StarFate.Nebula)
        {
            GameObject whiteDwarf = Instantiate(Manager.Instance.LoadedPrefabs.WhiteDwarfWithNebula, transform.position, Quaternion.identity);
            WhiteDwarf whiteDwarfComp = whiteDwarf.GetComponent<WhiteDwarf>();
            whiteDwarfComp.InitializeCelestialObject(scale);
            whiteDwarfComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            whiteDwarfComp.InitializeWhiteDwarf(newBaseAge, newAge, scale * Random.Range(4f, 6f), Random.Range(-20f, 20f));
            whiteDwarf.GetComponent<NetworkObject>().Spawn();
            Destroy(gameObject);
            return;
        }
        // supernova
        SupernovaRpc(scale, default);

        float maxDistance = 50f * scale; // max distance for supernova effect
        NovaScreenShakeRpc(maxDistance, default);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxDistance, LayerMask.GetMask("Player"));
        foreach (Collider2D collider in colliders)
        {
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            // linear dropoff (instead of inverse distance squared) so it's not as extreme
            float forceDistAdjustment = (1 - (Vector2.Distance(transform.position, collider.transform.position) / maxDistance));
            rb.AddForce(Manager.Instance.SupernovaForce * scale * forceDistAdjustment * (collider.transform.position - transform.position).normalized, ForceMode2D.Impulse);
        }
        
        if (fate.Value == StarFate.NeutronStar)
        {
            GameObject neutronStar = Instantiate(Manager.Instance.LoadedPrefabs.NeutronStar, transform.position, Quaternion.identity);
            NeutronStar neutronStarComp = neutronStar.GetComponent<NeutronStar>();
            neutronStarComp.InitializeCelestialObject(scale);
            neutronStarComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            neutronStarComp.InitializeNeutronStar(newBaseAge, newAge, scale * Random.Range(250f, 350f), scale * Random.Range(0.8f, 1.2f));
            neutronStar.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            GameObject blackHole = Instantiate(Manager.Instance.LoadedPrefabs.BlackHole, transform.position, Quaternion.identity);
            BlackHole blackHoleComp = blackHole.GetComponent<BlackHole>();
            blackHoleComp.InitializeCelestialObject(scale);
            blackHoleComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            blackHoleComp.InitializeBlackHole(newBaseAge, newAge, scale * Random.Range(250f, 350f), scale * Random.Range(0.8f, 1.2f));
            blackHole.GetComponent<NetworkObject>().Spawn();
        }
        Destroy(gameObject);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SupernovaRpc(float scale, RpcParams rpcParams)
    {
        GameObject nova = Instantiate(ClientManager.Instance.LoadedPrefabs.SupernovaEffect, transform.position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        nova.transform.localScale = Vector2.one * 5 * scale;
        GameObject shockwave = Instantiate(ClientManager.Instance.LoadedPrefabs.SupernovaShockwave, transform.position, Quaternion.identity);
        shockwave.transform.localScale = Vector2.one * 10 * scale;
    }

    // client side
    [Rpc(SendTo.ClientsAndHost)]
    void NovaScreenShakeRpc(float maxDistance, RpcParams rpcParams)
    {
        Transform player = ClientManager.Instance.AstralProjecting ? ClientManager.Instance.Soul.transform : ClientManager.Instance.PlayerBall.transform;
        float strength = Mathf.Clamp(1 - (Vector2.Distance(player.position, transform.position) / maxDistance), 0, 1);
        if (strength > 0) ClientManager.Instance.TriggerScreenShake(strength);
    }

    void LateUpdate()
    {
        if (!IsServer) return;
        EvolutionProjection();
    }

    void EvolutionProjection()
    {
        if (type.Value == StarType.MainSequence || fate.Value == StarFate.Nebula)
        {
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
            Destroy(warning);
            Destroy(projection);
        }
    }

    public override string GetDescription()
    {
        if (type.Value == StarType.RedGiant)
        {
            return "Red Giant (Unstable)";
        }
        return "Star (Normal)";
    }
}
