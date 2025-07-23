using UnityEngine;

[CreateAssetMenu(fileName = "TimeDistortionData", menuName = "Scriptable Objects/TimeDistortionData")]

public class TimeDistortionData : PowerupData
{
    public float duration;

    public override Powerup GetPowerup()
    {
        return new TimeDistortion(powerupName, duration);
    }
}
