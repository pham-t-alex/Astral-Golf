using System.Collections.Generic;
using System.Linq;
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
    public void HandleTimeForward(bool accelerated)
    {
        timeDistortionButtonPressed = true;
        Messenger.Instance.StartTimeDistortion(new Manager.TimeDistortion(Manager.TimeDistortionType.Time, accelerated, true));
    }

    public void HandleTimeBackward(bool accelerated)
    {
        timeDistortionButtonPressed = true;
        Messenger.Instance.StartTimeDistortion(new Manager.TimeDistortion(Manager.TimeDistortionType.Time, accelerated, false));
    }

    public void HandleOrbitForward(bool accelerated)
    {
        timeDistortionButtonPressed = true;
        Messenger.Instance.StartTimeDistortion(new Manager.TimeDistortion(Manager.TimeDistortionType.Orbit, accelerated, true));
    }

    public void HandleOrbitBackward(bool accelerated)
    {
        timeDistortionButtonPressed = true;
        Messenger.Instance.StartTimeDistortion(new Manager.TimeDistortion(Manager.TimeDistortionType.Orbit, accelerated, false));
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
}