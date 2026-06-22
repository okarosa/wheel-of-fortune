using UnityEngine;
using System.Collections.Generic;

public class RewardCollector : MonoBehaviour
{
    public static RewardCollector Instance { get; private set; }

    public int TotalCoins      { get; set; }
    public int TotalMoney      { get; private set; }
    public int RifleCount      { get; private set; }
    public int ArmorCount      { get; private set; }
    public int KnifeCount      { get; private set; }
    public int PistolCount     { get; private set; }
    public int SubmachineCount { get; private set; }
    public int PumpgunCount    { get; private set; }
    public int SniperCount     { get; private set; }

    public List<WheelSliceData> CollectedRewards { get; private set; }

    private Dictionary<string, System.Action<int>> _adders;
    private Dictionary<string, string> _prefsKeys;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CollectedRewards = new List<WheelSliceData>();

        _adders = new Dictionary<string, System.Action<int>>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "coin",        v => TotalCoins      += v },
            { "coins",       v => TotalCoins      += v },
            { "money",       v => TotalMoney      += v },
            { "rifle",       v => RifleCount      += v },
            { "armor",       v => ArmorCount      += v },
            { "knife",       v => KnifeCount      += v },
            { "pistol",      v => PistolCount     += v },
            { "submachine",  v => SubmachineCount += v },
            { "shotgun",     v => PumpgunCount    += v },
            { "sniper",      v => SniperCount     += v },
        };

        _prefsKeys = new Dictionary<string, string>
        {
            { "coin",        "TotalCoins"      },
            { "money",       "TotalMoney"      },
            { "rifle",       "TotalRifle"      },
            { "armor",       "TotalArmor"      },
            { "knife",       "TotalKnife"      },
            { "pistol",      "TotalPistol"     },
            { "submachine",  "TotalSubmachine" },
            { "shotgun",     "TotalPumpgun"    },
            { "sniper",      "TotalSniper"     },
        };
    }

    public void AddReward(WheelSliceData slice)
    {
        CollectedRewards.Add(slice);
        if (_adders.TryGetValue(slice.rewardName, out var add))
            add(slice.rewardAmount);
    }

    public void ResetAll()
    {
        CollectedRewards.Clear();
        TotalCoins = TotalMoney = RifleCount = ArmorCount = KnifeCount = 0;
        PistolCount = SubmachineCount = PumpgunCount = SniperCount = 0;
    }

    public void CashOut()
    {
        if (CollectedRewards.Count == 0) return;

        var session = new Dictionary<string, int>
        {
            { "coin",        TotalCoins      },
            { "money",       TotalMoney      },
            { "rifle",       RifleCount      },
            { "armor",       ArmorCount      },
            { "knife",       KnifeCount      },
            { "pistol",      PistolCount     },
            { "submachine",  SubmachineCount },
            { "shotgun",     PumpgunCount    },
            { "sniper",      SniperCount     },
        };

        foreach (var entry in session)
        {
            if (_prefsKeys.TryGetValue(entry.Key, out var key))
                PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + entry.Value);
        }

        PlayerPrefs.Save();
        ResetAll();
    }
}