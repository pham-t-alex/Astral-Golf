using UnityEngine;

public class OrbitShift : Powerup
{
    public float timeLeft;
    public float maxTimeLeft;

    public OrbitShift(string name, float duration) : base(name)
    {
        timeLeft = duration;
        maxTimeLeft = duration;
    }
}
