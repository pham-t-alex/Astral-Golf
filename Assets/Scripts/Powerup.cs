using UnityEngine;

public abstract class Powerup
{
    private string powerupName;
    public string PowerupName => powerupName;

    public Powerup(string name)
    {
        powerupName = name;
    }
}
