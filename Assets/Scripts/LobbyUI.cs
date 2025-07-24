using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    private static LobbyUI instance;
    public static LobbyUI Instance => instance;
    [SerializeField] private TMP_InputField createLobbyNameInput;
    [SerializeField] private Button createLobbyButton;
    private string lobbyCreationName = "";

    private LoadedLobby selectedLobby;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject loadedLobby;
    [SerializeField] private TMP_Text currentLobbyText;
    private string username = "";

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SessionManager.Instance.LobbiesLoaded += UpdateLobbies;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetLobbyCreationName(string name)
    {
        lobbyCreationName = name;
    }

    public async void CreateLobby()
    {
        if (lobbyCreationName == null || lobbyCreationName == "") return;
        await SessionManager.Instance.StartSessionAsHost(lobbyCreationName);
        createLobbyNameInput.text = "";
        createLobbyNameInput.enabled = false;
        lobbyCreationName = "";
        createLobbyButton.enabled = false;
        UpdateCurrentLobby();
    }

    public void SelectLobby(LoadedLobby lobby)
    {
        if (selectedLobby != null)
        {
            selectedLobby.SetImageColor(Color.white);
        }
        selectedLobby = lobby;
        selectedLobby.SetImageColor(Color.yellow);
    }

    public async void JoinLobby()
    {
        if (selectedLobby == null) return;
        await SessionManager.Instance.JoinSessionById(selectedLobby.Id);
        createLobbyNameInput.text = "";
        createLobbyNameInput.enabled = true;
        lobbyCreationName = "";
        createLobbyButton.enabled = true;
        UpdateCurrentLobby();
    }

    public void UpdateLobbies()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        IList<ISessionInfo> sessions = SessionManager.Instance.SessionInfo;

        content.sizeDelta = new Vector2(content.sizeDelta.x, 10 + (sessions.Count * 110));
        foreach (ISessionInfo session in sessions)
        {
            GameObject g = Instantiate(loadedLobby, content);
            LoadedLobby lobby = g.GetComponent<LoadedLobby>();
            lobby.SetImageColor(Color.white);
            lobby.SetId(session.Id);
            lobby.SetLobbyName(session.Name);
            lobby.SetPlayerCountText(session.AvailableSlots, session.MaxPlayers);
        }
    }

    public void UpdateCurrentLobby()
    {
        ISession session = SessionManager.Instance.ActiveSession;
        UpdateCurrentLobbyText();
        session.PlayerJoined += (val) => UpdateCurrentLobbyText();
        session.PlayerLeaving += (val) => UpdateCurrentLobbyText();
    }

    public void UpdateCurrentLobbyText()
    {
        ISession session = SessionManager.Instance.ActiveSession;
        currentLobbyText.text = $"Lobby: {session.Name}\nPlayers: {session.PlayerCount}/{session.MaxPlayers}";
    }

    public void StartGame()
    {
        ISession session = SessionManager.Instance.ActiveSession;
        if (session != null && session.IsHost && session.PlayerCount > 1 && NetworkManager.Singleton.ConnectedClientsIds.Count > 1)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Space", LoadSceneMode.Single);
        }
    }

    public async void RefreshPublicLobbies()
    {
        await SessionManager.Instance.FetchLobbies();
    }
}
