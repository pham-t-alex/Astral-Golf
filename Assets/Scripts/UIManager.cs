using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;
    [SerializeField] private GameObject turnIndicator;

    [SerializeField] private Image[] powerupImages;
    private List<string> powerups = new List<string>();

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

    private void Awake()
    {
        if (!NetworkManager.Singleton.IsClient)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private bool timeDistortionButtonPressed = false;
    private string selectedPowerup = "Time Distortion";

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
        if (ClientManager.Instance.AstralProjecting)
        {
            astralMode.SetActive(true);
        }
        else
        {
            astralMode.SetActive(false);
        }
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
                powerupImages[i].sprite = GetPowerupSpriteByName(name);
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
        // implement
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
}