using UnityEngine;

[CreateAssetMenu(fileName = "ExtraLifeData", menuName = "Scriptable Objects/ExtraLifeData")]

public class ExtraLifeData : PowerupData
{
    public override Powerup GetPowerup()
    {
        return new ExtraLife(powerupName);
    }
}
