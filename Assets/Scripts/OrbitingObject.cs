using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class OrbitingObject : CelestialObject
{
    protected Vector2 orbitCenter;
    protected float semiMajorAxisLength;
    protected float semiMinorAxisLength;
    protected float ellipticalRotation;
    protected float startingAngle;
    protected float angularVelocity;
    
    private Rigidbody2D rb;
    private List<GameObject> orbitLines = new List<GameObject>();

    public void InitializeOrbit(Vector2 orbitCenter, float semiMajor, float semiMinor, float rotation, float angle, float angularVelocity)
    {
        this.orbitCenter = orbitCenter;
        semiMajorAxisLength = semiMajor;
        semiMinorAxisLength = semiMinor;
        ellipticalRotation = rotation;
        startingAngle = angle;
        this.angularVelocity = angularVelocity;

        rb = GetComponent<Rigidbody2D>();
    }

    private void UpdateOrbit(float orbitTime)
    {
        float angle = startingAngle + (orbitTime * angularVelocity);
        if (rb == null) return;
        rb.MovePosition(EllipsePosition(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * angle));
    }

    protected override void StartServerSetup()
    {
        base.StartServerSetup();
        SetupOrbitClientRpc(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, default);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetupOrbitClientRpc(Vector2 center, float semiMajor, float semiMinor, float rotation, RpcParams rpcParams)
    {
        Vector2 prevPoint = EllipsePosition(center, semiMajor, semiMinor, rotation, 0);
        int segments = ClientManager.Instance.OrbitSegments;
        for (int i = 1; i <= segments; i++)
        {
            float newAngle = ((float)i / segments) * 2 * Mathf.PI;
            Vector2 point = EllipsePosition(center, semiMajor, semiMinor, rotation, newAngle);
            GameObject g = Instantiate(ClientManager.Instance.LoadedPrefabs.StarOrbitLine, prevPoint, Quaternion.identity);
            g.GetComponent<LineRenderer>().SetPosition(1, point - prevPoint);
            orbitLines.Add(g);
            prevPoint = point;
        }
    }

    protected override void StartClientSetup()
    {
        base.StartClientSetup();
        if (!IsServer) Destroy(GetComponent<Rigidbody2D>());
    }

    protected override void ServerFixedTick(float fixedDeltaTime)
    {
        base.ServerFixedTick(fixedDeltaTime);
        float orbitTime = Manager.Instance.OrbitTime;
        UpdateOrbit(orbitTime);
    }

    public static Vector2 EllipsePosition(Vector2 center, float semiMajor, float semiMinor, float rotation, float radAngle)
    {
        Vector2 unrotatedPosRelativeToCenter = new Vector2(semiMajor * Mathf.Cos(radAngle), semiMinor * Mathf.Sin(radAngle));
        Vector2 rotatedPosRelativeToCenter = Quaternion.Euler(0, 0, rotation) * unrotatedPosRelativeToCenter;
        return center + rotatedPosRelativeToCenter;
    }

    public static Vector2 EllipseVelocity(float semiMajor, float semiMinor, float rotation, float radAngle, float angularVelocity)
    {
        Vector2 direction = new Vector2(-semiMajor * Mathf.Sin(radAngle), semiMinor * Mathf.Cos(radAngle));
        Vector2 rotatedDirection = Quaternion.Euler(0, 0, rotation) * direction;
        return angularVelocity * rotatedDirection;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!IsClient) return;
        foreach (GameObject line in orbitLines)
        {
            Destroy(line);
        }
    }
}