using UnityEngine;

public class NovaShockwave : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    private Material material;
    [SerializeField] private float minShockwaveDist = -0.1f;
    [SerializeField] private float maxShockwaveDist = 0.56f;
    private float time;

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        material.SetFloat("_ShockwaveTravelDistance", Mathf.Lerp(minShockwaveDist, maxShockwaveDist, time / duration));
        if (time > duration)
        {
            Destroy(gameObject);
        }
    }
}
