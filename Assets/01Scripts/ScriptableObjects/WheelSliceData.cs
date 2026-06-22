using UnityEngine;

[CreateAssetMenu(fileName = "WheelSliceData", menuName = "WheelOfFortune/WheelSliceData", order = 1)]
public class WheelSliceData : ScriptableObject
{
    public Sprite icon;
    public string rewardName;
    public int rewardAmount;
    public bool isBomb;
}
