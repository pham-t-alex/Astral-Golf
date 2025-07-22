using UnityEngine;

[CreateAssetMenu(fileName = "LoadedSprites", menuName = "Scriptable Objects/LoadedSprites")]
public class LoadedSprites : ScriptableObject
{
    [SerializeField] private Sprite blueStar;
    public Sprite BlueStar => blueStar;
}
