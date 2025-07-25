using Unity.Netcode;
using UnityEngine;

public class GoalHole : MonoBehaviour
{
    private static GoalHole instance;
    public static GoalHole Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (!NetworkManager.Singleton.IsServer) Destroy(GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        Manager.Instance.PlayerReachedGoal(collision.GetComponent<PlayerBall>());
    }
}
