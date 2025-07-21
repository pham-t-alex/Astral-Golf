using Unity.Netcode;
using UnityEngine;
using static Manager;

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

    // Client -> Server, notifies server that a client is beginning time distortion
    public void StartTimeDistortion(Manager.TimeDistortion timeDistortion)
    {
        StartTimeDistortionRpc(timeDistortion, default);
    }

    [Rpc(SendTo.Server)]
    void StartTimeDistortionRpc(Manager.TimeDistortion timeDistortion, RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Manager.Instance.StartTimeDistortion(clientId, timeDistortion);
    }

    // Client -> Server, notifies server that a client is ending time distortion
    public void EndTimeDistortion()
    {
        EndTimeDistortionRpc(default);
    }

    [Rpc(SendTo.Server)]
    void EndTimeDistortionRpc(RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Manager.Instance.EndTimeDistortion(clientId);
    }
}
