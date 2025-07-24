using UnityEngine;

public class InfoFollow : MonoBehaviour
{
    private Transform target;

    private void Start()
    {
        UIManager.Instance.InitializeWorldInfoText(gameObject);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        transform.position = target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        transform.position = target.position;
    }
}