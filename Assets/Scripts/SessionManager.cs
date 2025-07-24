using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private static SessionManager instance;
    public static SessionManager Instance => instance;

    private ISession activeSession;
    public ISession ActiveSession => activeSession;

    private IList<ISessionInfo> sessionInfo;
    public IList<ISessionInfo> SessionInfo => sessionInfo;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public event Action LobbiesLoaded;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed In: " + AuthenticationService.Instance.PlayerId);
            await FetchLobbies();
            
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task StartSessionAsHost(string name)
    {
        if (activeSession != null)
        {
            await activeSession.LeaveAsync();
        }
        SessionOptions options = new SessionOptions
        {
            Name = name,
            MaxPlayers = 10,
            IsLocked = false,
            IsPrivate = false
        }.WithRelayNetwork();

        activeSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {activeSession.Id} created! Join code: {activeSession.Code}");
    }

    public async Task JoinSessionById(string id)
    {
        if (activeSession != null)
        {
            await activeSession.LeaveAsync();
        }
        activeSession = await MultiplayerService.Instance.JoinSessionByIdAsync(id);
        Debug.Log($"Joined session {activeSession.Id}");
    }

    public async Task FetchLobbies()
    {
        QuerySessionsOptions options = new QuerySessionsOptions
        {
            Count = 10
        };
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(options);
        sessionInfo = results.Sessions;
        LobbiesLoaded?.Invoke();
    }
}
