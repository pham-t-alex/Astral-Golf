using UnityEngine;

public class TimeDistortion : Powerup
{
    public float timeLeft;
    public float maxTimeLeft;

    public TimeDistortion(string name, float duration) : base(name)
    {
        timeLeft = duration;
        maxTimeLeft = duration;
    }
}
