using UnityEngine;

public abstract class OrbitingObject : CelestialObject
{
    private Vector2 orbitCenter;
    private float orbitRadius;
    private float startingAngle;
    private float angularVelocity;
    private Rigidbody2D rb;

    public void InitializeOrbit(Vector2 orbitCenter, float orbitRadius, float angle, float angularVelocity)
    {
        this.orbitCenter = orbitCenter;
        this.orbitRadius = orbitRadius;
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
        rb.MovePosition(orbitCenter + orbitRadius * (new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle))));
    }

    protected override void FixedTick(float fixedDeltaTime)
    {
        base.FixedTick(fixedDeltaTime);
        float orbitTime = Manager.Instance.OrbitTime;
        UpdateOrbit(orbitTime);
    }
}