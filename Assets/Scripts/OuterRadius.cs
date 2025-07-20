using UnityEngine;
using System;

public class OuterRadius : MonoBehaviour
{
    public event Action<Collider2D> OnOuterRadiusEnter;
    public event Action<Collider2D> OnOuterRadiusExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnOuterRadiusEnter?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        OnOuterRadiusExit?.Invoke(collision);
    }
}
