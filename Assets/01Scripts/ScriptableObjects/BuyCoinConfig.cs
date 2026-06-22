using UnityEngine;

[System.Serializable]
public class BuyCoinOption
{
    //public string label;
    //public int price;
    public int coinReward;
}

[CreateAssetMenu(fileName = "BuyCoinConfig", menuName = "WheelOfFortune/BuyCoinConfig", order = 2)]
public class BuyCoinConfig : ScriptableObject
{
    public BuyCoinOption[] options;
}
