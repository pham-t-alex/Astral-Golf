using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadedLobby : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Image image;
    private string id;
    public string Id => id;

    public void SetId(string id)
    {
        this.id = id;
    }

    public void SetLobbyName(string name)
    {
        nameText.text = $"Lobby: {name}";
    }

    public void SetPlayerCountText(int available, int maxPlayers)
    {
        playerCountText.text = $"Players: {maxPlayers - available}/{maxPlayers}";
    }

    public void SetImageColor(Color c)
    {
        image.color = c;
    }

    public void SelectLobby()
    {
        LobbyUI.Instance.SelectLobby(this);
    }
}
