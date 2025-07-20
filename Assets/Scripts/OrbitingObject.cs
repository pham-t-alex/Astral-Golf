using UnityEngine;

public abstract class OrbitingObject : CelestialObject
{
    private Vector2 orbitCenter;
    private float semiMajorAxisLength;
    private float semiMinorAxisLength;
    private float ellipticalRotation;
    private float startingAngle;
    private float angularVelocity;
    private Rigidbody2D rb;

    public void InitializeOrbit(Vector2 orbitCenter, float semiMajor, float semiMinor, float rotation, float angle, float angularVelocity)
    {
        this.orbitCenter = orbitCenter;
        semiMajorAxisLength = semiMajor;
        semiMinorAxisLength = semiMinor;
        ellipticalRotation = rotation;
        startingAngle = angle;
        this.angularVelocity = angularVelocity;
    }

    protected override void StartSetup()
    {
        base.StartSetup();
        Manager.Instance.OrbitTimeUpdate += UpdateOrbit;
        rb = GetComponent<Rigidbody2D>();
    }

    private void UpdateOrbit(float orbitTime)
    {
        float angle = startingAngle + (orbitTime * angularVelocity);
        rb.MovePosition(EllipsePosition(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * angle));
    }

    protected override void FixedTick(float fixedDeltaTime)
    {
        base.FixedTick(fixedDeltaTime);
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
}