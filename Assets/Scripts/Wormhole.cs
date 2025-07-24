using UnityEngine;

public class Wormhole : CelestialObject
{
    [Header("Wormhole Settings")]
    [SerializeField] private Wormhole destinationWormhole;

    private void OnDrawGizmos()
    {
        Gizmos.color = GetComponent<SpriteRenderer>().color;
        Gizmos.DrawSphere(transform.position, 0.4f * scale);
        if (destinationWormhole == null) return;
        Gizmos.DrawLine(transform.position, destinationWormhole.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        PlayerBall player = collision.GetComponent<PlayerBall>();
        if (player.ExitWormhole == this) return;
        player.SetExitWormhole(destinationWormhole);
        player.transform.position = destinationWormhole.transform.position;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        PlayerBall player = collision.GetComponent<PlayerBall>();
        if (player.ExitWormhole == this) player.SetExitWormhole(null);
    }

    protected override void StartServerSetup()
    {
        InitializeCelestialObject(scale);
        if (!IsClient) Destroy(GetComponent<SpriteRenderer>());
    }

    public override string GetDescription()
    {
        return "Wormhole (Teleport)";
    }
}
