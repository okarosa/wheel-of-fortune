using UnityEngine;

[CreateAssetMenu(fileName = "WheelConfigData", menuName = "WheelOfFortune/WheelConfigData", order = 2)]
public class WheelConfigData : ScriptableObject
{
    //public string wheelName;
    public WheelSliceData[] rewards;
    public bool hasBomb = true;
    public Sprite wheelBackground;
    public Sprite wheelPointerSprite;
    public Color wheelBgColor = Color.white;
    public AudioClip backgroundMusic;
}
