using TMPro;
using UnityEngine;

public class TextFollow : InfoFollow
{
    [SerializeField] private TMP_Text text;
    
    public void SetText(string text)
    {
        this.text.text = text;
    }
}