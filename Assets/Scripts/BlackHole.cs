using System.Collections.Generic;
using UnityEngine;

public class BlackHole : OrbitingObject
{
    [SerializeField] private GameObject outerTrigger;
    private float pullForceFactor;
    private HashSet<PlayerBall> players = new HashSet<PlayerBall>();
    public void InitializeBlackHole(float pullForceFactor, float pullScale)
    {
        this.pullForceFactor = pullForceFactor;
        outerTrigger.transform.localScale = new Vector3(pullScale / transform.localScale.x, pullScale / transform.localScale.y, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        Destroy(collision.gameObject);
    }

    private void FixedUpdate()
    {
        foreach (PlayerBall player in players)
        {
            Vector2 direction = (transform.position - player.transform.position).normalized;
            float distance = Vector2.Distance(transform.position, player.transform.position);
            float pullForce = pullForceFactor / (distance * distance);
            player.GetComponent<Rigidbody2D>().AddForce(direction * pullForce, ForceMode2D.Force);
        }
    }

    protected override void StartSetup()
    {
        base.StartSetup();
        OuterRadius outerRadius = outerTrigger.GetComponent<OuterRadius>();
        outerRadius.OnOuterRadiusEnter += (collider) =>
        {
            if (collider.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            players.Add(collider.GetComponent<PlayerBall>());
        };
        outerRadius.OnOuterRadiusExit += (collider) =>
        {
            if (collider.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            players.Remove(collider.GetComponent<PlayerBall>());
        };
    }
}
