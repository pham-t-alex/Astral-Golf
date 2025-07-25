using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;
    [SerializeField] private GameObject rankIndicator;
    [SerializeField] private GameObject turnIndicator;

    [SerializeField] private Image[] powerupImages;
    private List<string> powerups = new List<string>();
    public List<string> Powerups => powerups;

    [SerializeField] private GameObject worldInfoTexts;
    [SerializeField] private GameObject uiInfoTexts;
    private bool infoTextsActive = false;

    [SerializeField] private GameObject astralMode;
    [SerializeField] private GameObject batteryText;
    [SerializeField] private GameObject orbitModel;
    [SerializeField] private GameObject timeModel;
    [SerializeField] private GameObject distortionButtons;
    [SerializeField] private GameObject orbitShiftInfo;
    [SerializeField] private GameObject timeDistortionInfo;
    [SerializeField] private Button astralProjectionButton;

    [SerializeField] private GameObject gameEndScreen;
    [SerializeField] private GameObject gameEndUI;
    [SerializeField] private GameObject gameEndRankText;

    [SerializeField] private GameObject goalArrow;
    [SerializeField] private float arrowOffset;
    [SerializeField] private GameObject cameraRoot;

    private void Awake()
    {
        if (!NetworkManager.Singleton.IsClient)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void LateUpdate()
    {
        UpdateGoalArrow();
    }

    void UpdateGoalArrow()
    {
        Vector2 goalCanvasPos = Camera.main.WorldToScreenPoint(GoalHole.Instance.transform.position);
        if (0 <= goalCanvasPos.x && goalCanvasPos.x <= Screen.width
            && 0 <= goalCanvasPos.y && goalCanvasPos.y <= Screen.height)
        {
            goalArrow.SetActive(false);
            return;
        }
        Vector2 direction = (GoalHole.Instance.transform.position - cameraRoot.transform.position).normalized;
        Rect canvasRect = GetComponent<RectTransform>().rect;
        float halfWidth = (canvasRect.width - (2 * arrowOffset)) / 2;
        float halfHeight = (canvasRect.height - (2 * arrowOffset)) / 2;
        float scale = Mathf.Min(halfWidth / Mathf.Abs(direction.x), halfHeight / Mathf.Abs(direction.y));

        Vector2 position = new Vector2(scale * direction.x, scale * direction.y);

        goalArrow.GetComponent<RectTransform>().anchoredPosition = position;
        goalArrow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        goalArrow.SetActive(true);
    }

    private bool timeDistortionButtonPressed = false;
    private string selectedPowerup = null;

    public void HandleForward(bool accelerated)
    {
        Manager.TimeDistortionType type;
        if (selectedPowerup == "Time Distortion")
        {
            type = Manager.TimeDistortionType.Time;
        }
        else if (selectedPowerup == "Orbit Shift")
        {
            type = Manager.TimeDistortionType.Orbit;
        }
        else
        {
            return;
        }
        timeDistortionButtonPressed = true;
        Messenger.Instance.StartTimeDistortion(new Manager.TimeDistortion(type, accelerated, true));
    }

    public void HandleBackward(bool accelerated)
    {
        Manager.TimeDistortionType type;
        if (selectedPowerup == "Time Distortion")
        {
            type = Manager.TimeDistortionType.Time;
        }
        else if (selectedPowerup == "Orbit Shift")
        {
            type = Manager.TimeDistortionType.Orbit;
        }
        else
        {
            return;
        }
        timeDistortionButtonPressed = true;
        Messenger.Instance.StartTimeDistortion(new Manager.TimeDistortion(type, accelerated, false));
    }

    public void HandleDistortionReleased()
    {
        if (timeDistortionButtonPressed)
        {
            Messenger.Instance.EndTimeDistortion();
        }
        timeDistortionButtonPressed = false;
    }

    public void SetTurnIndicatorActive()
    {
        if (turnIndicator != null)
        {
            turnIndicator.SetActive(true);
        }
    }

    public void SetTurnIndicatorInactive()
    {
        if (turnIndicator != null)
        {
            turnIndicator.SetActive(false);
        }
    }

    public void AstralProject()
    {
        ClientManager.Instance.HandleAstralProject();
        astralMode.SetActive(ClientManager.Instance.AstralProjecting);
    }

    public void RemovePlayerBody()
    {
        astralMode.SetActive(true);
        astralProjectionButton.enabled = false;
        turnIndicator.SetActive(false);
    }

    public void PickupPowerup(string name)
    {
        if (powerups.Count == 3)
        {
            powerups.RemoveAt(0);
        }
        powerups.Add(name);

        for (int i = 0; i < powerupImages.Length; i++)
        {
            if (i > powerups.Count - 1)
            {
                powerupImages[i].sprite = null;
                powerupImages[i].enabled = false;
            }
            else
            {
                powerupImages[i].sprite = GetPowerupSpriteByName(powerups[i]);
                powerupImages[i].enabled = true;
            }
        }
    }

    Sprite GetPowerupSpriteByName(string name)
    {
        foreach (PowerupData data in ClientManager.Instance.Powerups)
        {
            if (data.powerupName == name)
            {
                return data.sprite;
            }
        }
        return null;
    }

    // On click with index
    public void SelectPowerup(int index)
    {
        if (index < 0 || index >= powerups.Count) return;

        // Send RPC to server to select powerup for this player
        Messenger.Instance.SelectPowerup(index);
    }

    public void UpdateSelectedPowerupUI(string powerup)
    {
        selectedPowerup = powerup;
        // Highlight selected powerup in UI, show info, etc.
        // Example: update info panels
        if (selectedPowerup == "Time Distortion")
        {
            timeModel.SetActive(true);
            orbitModel.SetActive(false);
            batteryText.SetActive(true);
            distortionButtons.SetActive(true);
        }
        else if (selectedPowerup == "Orbit Shift")
        {
            timeModel.SetActive(false);
            orbitModel.SetActive(true);
            batteryText.SetActive(true);
            distortionButtons.SetActive(true);
        }
        else
        {
            timeModel.SetActive(false);
            orbitModel.SetActive(false);
            batteryText.SetActive(false);
            distortionButtons.SetActive(false);
        }
        // You can add more UI logic here (e.g., highlight selected slot)
    }

    public void RemovePowerup(int index)
    {
        powerups.RemoveAt(index);
        for (int i = 0; i < powerupImages.Length; i++)
        {
            if (i > powerups.Count - 1)
            {
                powerupImages[i].sprite = null;
                powerupImages[i].enabled = false;
            }
            else
            {
                powerupImages[i].sprite = GetPowerupSpriteByName(powerups[i]);
                powerupImages[i].enabled = true;
            }
        }
    }

    public void InitializeWorldInfoText(GameObject text)
    {
        text.transform.SetParent(worldInfoTexts.transform);
    }

    public void ToggleInfoTexts()
    {
        infoTextsActive = !infoTextsActive;
        worldInfoTexts.SetActive(infoTextsActive);
        uiInfoTexts.SetActive(infoTextsActive);

        if (selectedPowerup == "Time Distortion")
        {
            timeDistortionInfo.SetActive(infoTextsActive);
        }
        else if (selectedPowerup == "Orbit Shift")
        {
            orbitShiftInfo.SetActive(infoTextsActive);
        }
    }

    public void PlayerVictory(int rank)
    {
        rankIndicator.SetActive(true);
        rankIndicator.GetComponent<TMP_Text>().text = "Rank " + rank;
    }

    public IEnumerator GameEnd(int rank)
    {
        gameEndScreen.SetActive(true);
        float elapsed = 0f;
        while (elapsed < 2)
        {
            elapsed += Time.deltaTime;
            gameEndScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f * (Mathf.Min(elapsed, 2) / 2));
            yield return null;
        }
        gameEndRankText.GetComponent<TMP_Text>().text = rank > 0 ? $"Rank: {rank}" : "Unranked";
        gameEndUI.SetActive(true);
        yield return null;
    }

    public void UpdateBattery(int percentage)
    {
        batteryText.GetComponent<TMP_Text>().text = $"{percentage}%";
    }
}