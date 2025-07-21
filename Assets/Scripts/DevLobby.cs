using Unity.Netcode;
using UnityEngine;

public class DevLobby : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Space", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
