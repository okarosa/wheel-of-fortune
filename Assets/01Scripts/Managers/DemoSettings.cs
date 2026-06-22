using UnityEngine;

public class DemoSettings : MonoBehaviour
{
    public int startingCoins = 100;
    public int startingMoney = 1000;

    private static readonly string[] WeaponKeys =
    {
        "TotalRifle", "TotalPistol", "TotalSubmachine",
        "TotalShotgun", "TotalSniper", "TotalArmor", "TotalKnife"
    };

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("TotalCoins")) PlayerPrefs.SetInt("TotalCoins", startingCoins);
        if (!PlayerPrefs.HasKey("TotalMoney")) PlayerPrefs.SetInt("TotalMoney", startingMoney);

        foreach (var key in WeaponKeys)
            if (!PlayerPrefs.HasKey(key)) PlayerPrefs.SetInt(key, 0);

        PlayerPrefs.Save();
    }

    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[DemoSettings] All PlayerPrefs cleared.");
    }

    public void ResetDefaults()
    {
        ClearAllPlayerPrefs();
        PlayerPrefs.SetInt("TotalCoins", startingCoins);
        PlayerPrefs.SetInt("TotalMoney", startingMoney);
        PlayerPrefs.Save();
        Debug.Log($"[DemoSettings] Reset to defaults: Coins={startingCoins}, Money={startingMoney}");
    }
}