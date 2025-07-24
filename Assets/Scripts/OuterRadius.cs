using UnityEngine;
using System;
using Unity.Netcode;

public class OuterRadius : MonoBehaviour
{
    public event Action<Collider2D> OnOuterRadiusEnter;
    public event Action<Collider2D> OnOuterRadiusExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        OnOuterRadiusEnter?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        OnOuterRadiusExit?.Invoke(collision);
    }
}
