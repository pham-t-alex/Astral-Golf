using Unity.Netcode;
using UnityEngine;

public abstract class CelestialObject : NetworkBehaviour
{
    [Header("Celestial Object Settings")]
    [SerializeField] protected float scale;
    private TextFollow infoText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            StartServerSetup();
        }
        if (IsClient)
        {
            StartClientSetup();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        ServerFixedTick(Time.fixedDeltaTime);
    }

    public void InitializeCelestialObject(float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1);
        this.scale = scale;
    }

    protected virtual void Tick(float deltaTime) { }
    protected virtual void StartServerSetup() { }
    protected virtual void StartClientSetup()
    {
        if (!IsServer) Destroy(GetComponent<Collider2D>());
        infoText = Instantiate(ClientManager.Instance.LoadedPrefabs.FollowText).GetComponent<TextFollow>();
        infoText.SetTarget(transform);
        infoText.SetText(GetDescription());
    }
    protected virtual void ServerFixedTick(float fixedDeltaTime) { }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!IsClient) return;
        Destroy(infoText.gameObject);
    }

    public virtual string GetDescription()
    {
        return "";
    }
}
