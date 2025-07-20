using System.Collections.Generic;
using UnityEngine;
using static Star;

public class BlackHole : OrbitingObject
{
    private float baseAge;
    private float age = 0;
    private bool destroyed = false;
    [SerializeField] private GameObject outerTrigger;
    private float pullForceFactor;
    private HashSet<PlayerBall> players = new HashSet<PlayerBall>();
    public void InitializeBlackHole(float baseAge, float age, float pullForceFactor, float pullScale)
    {
        this.baseAge = baseAge;
        this.age = age;
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
        Manager.Instance.GameTimeUpdate += UpdateBlackHoleAge;
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

    public void UpdateBlackHoleAge(float time)
    {
        if (destroyed) return;
        age = baseAge + time;
        if (age < 0)
        {
            destroyed = true;
            float newAge = age + Manager.Instance.RedGiantMaxAge;
            float newBaseAge = newAge - time;
            GameObject newStar = Instantiate(Manager.Instance.LoadedPrefabs.RedGiantStar, transform.position, Quaternion.identity);
            Star starComp = newStar.GetComponent<Star>();
            starComp.InitializeCelestialObject(scale);
            starComp.InitializeOrbit(orbitCenter, semiMajorAxisLength, semiMinorAxisLength, ellipticalRotation, startingAngle, angularVelocity);
            starComp.InitializeStar(newBaseAge, newAge, StarType.RedGiant, StarFate.BlackHole);
            Destroy(gameObject);
            return;
        }
    }
}
