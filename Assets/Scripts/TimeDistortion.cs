using UnityEngine;

public class TimeDistortion : Powerup
{
    private float timeLeft;

    public TimeDistortion(string name, float duration) : base(name)
    {
        timeLeft = duration;
    }
}
