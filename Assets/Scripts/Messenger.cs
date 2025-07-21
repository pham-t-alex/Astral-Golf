using UnityEngine;
using Unity.Netcode;

public class Messenger : NetworkBehaviour
{
    private static Messenger instance;
    public static Messenger Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Server -> Client, notifies client that it is their turn
    public void PlayerTurn(ulong clientId)
    {
        if (!IsServer) return;
        PlayerTurnRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void PlayerTurnRpc(RpcParams rpcParams)
    {
        ClientManager.Instance.StartTurn();
    }
}
