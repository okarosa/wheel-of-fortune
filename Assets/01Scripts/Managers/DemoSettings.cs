using UnityEngine;

public class DemoSettings : MonoBehaviour
{
    private static readonly string[] WeaponKeys =
    {
        "TotalRifle", "TotalPistol", "TotalSubmachine",
        "TotalShotgun", "TotalSniper", "TotalArmor", "TotalKnife"
    };

    private void Awake()
    {
        GameServices.Instance?.Register(this);
        if (!PlayerPrefs.HasKey("TotalCoins"))
            PlayerPrefs.SetInt("TotalCoins", 75);
        if (!PlayerPrefs.HasKey("TotalMoney"))
            PlayerPrefs.SetInt("TotalMoney", 1000);
        PlayerPrefs.Save();
    }

    public static int GetCoins() => PlayerPrefs.GetInt("TotalCoins", 100);
    public static int GetMoney() => PlayerPrefs.GetInt("TotalMoney", 1000);

    public static void ApplyDefaults(int coins, int money)
    {
        SetIfMissing("TotalCoins", coins);
        SetIfMissing("TotalMoney", money);
        PlayerPrefs.Save();
    }

    private static void SetIfMissing(string key, int value)
    {
        if (!PlayerPrefs.HasKey(key))
            PlayerPrefs.SetInt(key, value);
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[DemoSettings] All PlayerPrefs cleared.");
    }

    public static void SetDefaults(int coins, int money)
    {
        ClearAll();
        PlayerPrefs.SetInt("TotalCoins", coins);
        PlayerPrefs.SetInt("TotalMoney", money);
        PlayerPrefs.Save();
        Debug.Log($"[DemoSettings] Set defaults: Coins={coins}, Money={money}");
    }
}
