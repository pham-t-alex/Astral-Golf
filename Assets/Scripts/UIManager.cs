using Unity.Netcode;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;
    [SerializeField] private GameObject turnIndicator;

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
}