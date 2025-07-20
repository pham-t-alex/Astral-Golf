using UnityEngine;

public abstract class CelestialObject : MonoBehaviour
{
    [Header("Celestial Object Settings")]
    [SerializeField] protected float scale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartSetup();
    }

    // Update is called once per frame
    void Update()
    {
        Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        FixedTick(Time.fixedDeltaTime);
    }

    public void InitializeCelestialObject(float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1);
        this.scale = scale;
    }

    protected virtual void Tick(float deltaTime) { }
    protected virtual void StartSetup() { }
    protected virtual void FixedTick(float fixedDeltaTime) { }
    protected virtual void OnDestroy() { }
}
