using UnityEngine;

public class OrbitShift : Powerup
{
    private float timeLeft;

    public OrbitShift(string name, float duration) : base(name)
    {
        timeLeft = duration;
    }
}
