using UnityEngine;

public class Wormhole : CelestialObject
{
    [Header("Celestial Object Settings")]
    [SerializeField] private float scale = 1f;
    [Header("Wormhole Settings")]
    [SerializeField] private Wormhole destinationWormhole;

    private void OnDrawGizmos()
    {
        Gizmos.color = GetComponent<SpriteRenderer>().color;
        Gizmos.DrawSphere(transform.position, 0.5f * scale);
        if (destinationWormhole == null) return;
        Gizmos.DrawLine(transform.position, destinationWormhole.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        PlayerBall player = collision.GetComponent<PlayerBall>();
        if (player.ExitWormhole == this) return;
        player.SetExitWormhole(destinationWormhole);
        player.transform.position = destinationWormhole.transform.position;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        PlayerBall player = collision.GetComponent<PlayerBall>();
        if (player.ExitWormhole == this) player.SetExitWormhole(null);
    }

    protected override void StartSetup()
    {
        InitializeCelestialObject(scale);
    }
}
