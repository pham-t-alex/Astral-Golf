using UnityEngine;

[CreateAssetMenu(fileName = "OrbitShiftData", menuName = "Scriptable Objects/OrbitShiftData")]
public class OrbitShiftData : PowerupData
{
    public float duration;

    public override Powerup GetPowerup()
    {
        return new OrbitShift(powerupName, duration);
    }
}
