using UnityEngine;

public abstract class OrbitingObjectSpawn : CelestialObjectSpawn
{
    [Header("Orbit Settings")]
    [Tooltip("Semi major axis length")]
    [SerializeField] protected float semiMajorAxisLength = 1f;
    [Tooltip("Semi minor axis length")]
    [SerializeField] protected float semiMinorAxisLength = 1f;
    [Tooltip("Elliptical rotation in deg")]
    [Range(0, 359)]
    [SerializeField] protected float ellipticalRotation = 0f;
    [Tooltip("Starting angle in deg")]
    [Range(0, 359)]
    [SerializeField] protected float startingAngle;
    [Tooltip("Angular velocity in deg / sec")]
    [SerializeField] protected float angularVelocity = 5f;
    public Vector2 ObjPosition => OrbitingObject.EllipsePosition(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * startingAngle);

    // Draw Gizmos to visualize orbit and star
    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        // Draw orbit ellipse
        Gizmos.color = Color.white;
        int segments = 100;
        Vector2 prevPoint = OrbitingObject.EllipsePosition(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = ((float)i / segments) * 2 * Mathf.PI;
            Vector2 point = OrbitingObject.EllipsePosition(transform.position, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, angle);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // Draw star
        DrawObject(ObjPosition);

        // Draw starting angle line
        Gizmos.color = objColor;
        Gizmos.DrawLine(transform.position, ObjPosition);

        // Draw orbit velocity
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(ObjPosition, ObjPosition + 0.1f * (OrbitingObject.EllipseVelocity(semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, Mathf.Deg2Rad * startingAngle, angularVelocity)));
    }
}
