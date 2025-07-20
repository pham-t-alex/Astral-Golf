using UnityEngine;

public class NeutronStar : OrbitingObject
{
    private float pullForceFactor;
    private float maxPullRadius;
    public void InitializeNeutronStar(float pullForceFactor, float maxPullRadius)
    {
        this.pullForceFactor = pullForceFactor;
        this.maxPullRadius = maxPullRadius;
    }

    private void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxPullRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D collider in colliders)
        {
            Vector2 direction = (transform.position - collider.transform.position).normalized;
            float distance = Vector2.Distance(transform.position, collider.transform.position);
            float pullForce = pullForceFactor / (distance * distance);
            collider.GetComponent<Rigidbody2D>().AddForce(direction * pullForce, ForceMode2D.Force);
        }
    }
}
