using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool timeDistortionButtonPressed = false;
    public void HandleTimeForward(bool accelerated)
    {
        timeDistortionButtonPressed = true;
        Manager.Instance.StartTimeDistortion(new Manager.TimeDistortion(Manager.TimeDistortionType.Time, accelerated, true));
    }

    public void HandleTimeBackward(bool accelerated)
    {
        timeDistortionButtonPressed = true;
        Manager.Instance.StartTimeDistortion(new Manager.TimeDistortion(Manager.TimeDistortionType.Time, accelerated, false));
    }

    public void HandleOrbitForward(bool accelerated)
    {
        timeDistortionButtonPressed = true;
        Manager.Instance.StartTimeDistortion(new Manager.TimeDistortion(Manager.TimeDistortionType.Orbit, accelerated, true));
    }

    public void HandleOrbitBackward(bool accelerated)
    {
        timeDistortionButtonPressed = true;
        Manager.Instance.StartTimeDistortion(new Manager.TimeDistortion(Manager.TimeDistortionType.Orbit, accelerated, false));
    }

    public void HandleDistortionReleased()
    {
        if (timeDistortionButtonPressed)
        {
            Manager.Instance.StopTimeDistortion();
        }
        timeDistortionButtonPressed = false;
    }
}