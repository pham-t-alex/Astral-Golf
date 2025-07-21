using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Nebula : MonoBehaviour
{
    private float accelFactor;
    private HashSet<PlayerBall> playersInNebula = new HashSet<PlayerBall>();
    public void InitializeNebula(float accelFactor)
    {
        this.accelFactor = accelFactor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        playersInNebula.Add(collision.GetComponent<PlayerBall>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        playersInNebula.Remove(collision.GetComponent<PlayerBall>());
    }

    private void FixedUpdate()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        foreach (PlayerBall player in playersInNebula)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb.linearVelocity.magnitude < player.MoveVelocityThreshold) continue;
            Vector2 direction = rb.linearVelocity.normalized;
            rb.AddForce(direction * accelFactor, ForceMode2D.Force);
        }
    }
}
