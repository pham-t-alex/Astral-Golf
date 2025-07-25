using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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

    public void PlaySound(int idStart, int idEnd, Vector2 position)
    {
        PlaySoundRpc(idStart, idEnd, position, default);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlaySoundRpc(int idStart, int idEnd, Vector2 position, RpcParams rpcParams)
    {
        AudioManager.Instance.PlaySound(idStart, idEnd, position);
    }

    public void RemovePlayerBody(ulong clientId, Vector2 position)
    {
        RemovePlayerBodyRpc(position, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void RemovePlayerBodyRpc(Vector2 position, RpcParams rpcParams)
    {
        ClientManager.Instance.RemovePlayerBody(position);
    }

    public void PlayerVictory(ulong clientId, int rank)
    {
        PlayerVictoryRpc(rank, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void PlayerVictoryRpc(int rank, RpcParams rpcParams)
    {
        UIManager.Instance.PlayerVictory(rank);
    }

    public void EndGame(ulong clientId, int rank)
    {
        EndGameRpc(rank, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void EndGameRpc(int rank, RpcParams rpcParams)
    {
        StartCoroutine(UIManager.Instance.GameEnd(rank));
    }

    public void SelectPowerup(int index)
    {
        SelectPowerupRpc(index, default);
    }

    [Rpc(SendTo.Server)]
    public void SelectPowerupRpc(int index, RpcParams rpcParams)
    {
        Manager.Instance.SelectPowerup(rpcParams.Receive.SenderClientId, index);
    }

    public void UpdateSelectedPowerup(ulong clientId, string powerupName)
    {
        UpdateSelectedPowerupRpc(powerupName, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateSelectedPowerupRpc(string powerupName, RpcParams rpcParams)
    {
        UIManager.Instance.UpdateSelectedPowerupUI(powerupName);
    }

    public void RemovePowerup(ulong clientId, int index)
    {
        RemovePowerupRpc(index, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void RemovePowerupRpc(int index, RpcParams rpcParams)
    {
        UIManager.Instance.RemovePowerup(index);
    }

    public void UpdateBattery(ulong clientId, int percentage)
    {
        UpdateBatteryRpc(percentage, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void UpdateBatteryRpc(int percentage, RpcParams rpcParams)
    {
        UIManager.Instance.UpdateBattery(percentage);
    }
}
