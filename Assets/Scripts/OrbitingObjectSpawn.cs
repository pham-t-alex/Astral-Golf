using UnityEngine;

public abstract class OrbitingObjectSpawn : CelestialObjectSpawn
{
    [Header("Orbit Settings")]
    [Tooltip("Radius of orbit")]
    [SerializeField] protected float radius = 1f;
    [Tooltip("Starting angle in deg")]
    [Range(0, 359)]
    [SerializeField] protected float startingAngle;
    [Tooltip("Angular velocity in deg / sec")]
    [SerializeField] protected float angularVelocity;
    public Vector2 ObjPosition => (Vector2)transform.position + (radius * (new Vector2(Mathf.Cos(Mathf.Deg2Rad * startingAngle), Mathf.Sin(Mathf.Deg2Rad * startingAngle))));

    // Draw Gizmos to visualize orbit and star
    void OnDrawGizmosSelected()
    {
        // Draw orbit circle
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);

        // Draw star
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(ObjPosition, 0.5f * scale);

        // Draw starting angle line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, ObjPosition);
    }
}
