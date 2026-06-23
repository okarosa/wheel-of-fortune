using UnityEngine;
using UnityEditor;

public class PlayerSettingsWindow : EditorWindow
{
    private int _coins = 75;
    private int _money = 1000;
    private Vector2 _scroll;

    [MenuItem("Tools/Wheel of Fortune/Player Settings", priority = 100)]
    public static void ShowWindow()
    {
        var w = GetWindow<PlayerSettingsWindow>(true, "Player Settings", true);
        w.minSize = new Vector2(420, 240);
        w.LoadFromPrefs();
    }

    private void OnEnable()
    {
        LoadFromPrefs();
    }

    private void LoadFromPrefs()
    {
        _coins = PlayerPrefs.GetInt("TotalCoins", 75);
        _money = PlayerPrefs.GetInt("TotalMoney", 1000);
    }

    private void OnGUI()
    {
        var header = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
        var section = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };

        GUILayout.Space(8);
        GUILayout.Label("⚙ Player Starting Settings", header);
        GUILayout.Space(4);
        Rect r = EditorGUILayout.GetControlRect(false, 2);
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f, 0.3f));
        GUILayout.Space(8);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        GUILayout.Label("💰 Currency", section);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        _coins = EditorGUILayout.IntField("Starting Coins", Mathf.Max(0, _coins));
        _money = EditorGUILayout.IntField("Starting Money", Mathf.Max(0, _money));
        GUILayout.EndVertical();
        GUILayout.Space(12);

        var btnHeight = 34;
        GUILayout.BeginHorizontal();
        var applyColor = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.green }, fontSize = 12 };
        if (GUILayout.Button("✔ Apply to PlayerPrefs", applyColor, GUILayout.Height(btnHeight)))
            ApplySettings();
        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("↺ Reset to Defaults", GUILayout.Height(btnHeight)))
            ResetDefaults();
        if (GUILayout.Button("✕ Clear All", GUILayout.Height(btnHeight)))
            ClearAll();
        GUILayout.EndHorizontal();
        GUILayout.Space(16);

        GUILayout.Label("📋 Current PlayerPrefs", section);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        var valueStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
        GUILayout.BeginHorizontal();
        GUILayout.Label("Coins:", GUILayout.Width(50));
        GUILayout.Label($"{PlayerPrefs.GetInt("TotalCoins", 0):N0}", valueStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Money:", GUILayout.Width(55));
        GUILayout.Label($"{PlayerPrefs.GetInt("TotalMoney", 0):N0}", valueStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    private void ApplySettings()
    {
        PlayerPrefs.SetInt("TotalCoins", _coins);
        PlayerPrefs.SetInt("TotalMoney", _money);
        PlayerPrefs.Save();
        Debug.Log($"[PlayerSettings] Applied: Coins={_coins}, Money={_money}");
    }

    private void ResetDefaults()
    {
        if (!EditorUtility.DisplayDialog("Reset to Defaults",
            $"Reset coins to {_coins} and money to {_money}?", "Reset", "Cancel"))
            return;
        ApplySettings();
        Debug.Log("[PlayerSettings] Reset to defaults.");
    }

    private void ClearAll()
    {
        if (!EditorUtility.DisplayDialog("Clear All PlayerPrefs",
            "Are you sure you want to delete ALL saved data?", "Clear", "Cancel"))
            return;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        LoadFromPrefs();
        Repaint();
        Debug.Log("[PlayerSettings] All PlayerPrefs cleared.");
    }
}
