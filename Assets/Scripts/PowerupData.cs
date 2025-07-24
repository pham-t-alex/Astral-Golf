using UnityEngine;

public abstract class PowerupData : ScriptableObject
{
    public string powerupName;
    public Sprite sprite;

    public abstract Powerup GetPowerup();
}
