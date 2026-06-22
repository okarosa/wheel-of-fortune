using UnityEngine;
using UnityEditor;
using System.IO;

public class SliceDataCreatorWindow : EditorWindow
{
    private enum SpinType { Bronze, Silver, Gold }
    private enum RewardNameEnum { None, coin, money, armor, knife, rifle, pistol, submachine, shotgun, sniper, @case }

    private SpinType _spinType = SpinType.Bronze;
    private RewardNameEnum _rewardNameEnum = RewardNameEnum.coin;
    private string _customName = "";
    private int _amount = 10;
    private Sprite _icon;
    private Vector2 _scroll;

    [MenuItem("Tools/Wheel of Fortune/Slice Data Creator", priority = 110)]
    public static void ShowWindow()
    {
        var w = GetWindow<SliceDataCreatorWindow>(true, "Slice Data Creator", true);
        w.minSize = new Vector2(380, 320);
    }

    private string ResolveName()
    {
        if (_rewardNameEnum == RewardNameEnum.None)
            return _customName.ToLowerInvariant().Trim();
        return _rewardNameEnum.ToString().ToLowerInvariant();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        GUILayout.Label("Create New Slice Data", titleStyle);
        GUILayout.Space(15);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        _spinType = (SpinType)EditorGUILayout.EnumPopup("Spin Type", _spinType);
        GUILayout.Space(5);

        _rewardNameEnum = (RewardNameEnum)EditorGUILayout.EnumPopup("Reward Name", _rewardNameEnum);

        if (_rewardNameEnum == RewardNameEnum.None)
            _customName = EditorGUILayout.TextField("Custom Name", _customName);

        _amount = EditorGUILayout.IntField("Amount", Mathf.Max(1, _amount));
        _icon = (Sprite)EditorGUILayout.ObjectField("Icon", _icon, typeof(Sprite), false);

        GUILayout.Space(10);
        DrawPreview();
        GUILayout.Space(10);

        var canCreate = !string.IsNullOrEmpty(ResolveName()) && _amount > 0;
        GUI.enabled = canCreate;
        if (GUILayout.Button("Create Slice Data", GUILayout.Height(36)))
            CreateSliceData();
        GUI.enabled = true;

        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview()
    {
        var prefix = GetPrefix();
        var rewardName = ResolveName();
        var fileName = $"{prefix}_{rewardName}_{_amount}.asset";
        var folder = GetTargetFolder();

        var boxStyle = new GUIStyle(EditorStyles.helpBox);
        GUILayout.BeginVertical(boxStyle);
        var bold = new GUIStyle(EditorStyles.boldLabel);
        GUILayout.Label("Preview", bold);
        GUILayout.Label($"  File: {fileName}");
        GUILayout.Label($"  Path: {folder}/{fileName}");
        if (_icon != null)
        {
            var rt = GUILayoutUtility.GetRect(48, 48);
            GUI.DrawTexture(rt, _icon.texture, ScaleMode.ScaleToFit);
        }
        GUILayout.EndVertical();
    }

    private void CreateSliceData()
    {
        var folder = GetTargetFolder();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        var prefix = GetPrefix();
        var rewardName = ResolveName();
        var assetName = $"{prefix}_{rewardName}_{_amount}";
        var path = $"{folder}/{assetName}.asset";

        if (File.Exists(path))
        {
            if (!EditorUtility.DisplayDialog("Overwrite?",
                $"'{assetName}.asset' already exists.\nOverwrite it?", "Overwrite", "Cancel"))
                return;
            AssetDatabase.DeleteAsset(path);
        }

        var data = CreateInstance<WheelSliceData>();
        data.rewardName = rewardName;
        data.rewardAmount = _amount;
        data.icon = _icon;
        data.name = assetName;

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(data);
        Selection.activeObject = data;

        Debug.Log($"[SliceDataCreator] Created: {path}");

        OfferAddToConfig(data);

        _amount++;
        Repaint();
    }

    private string GetPrefix()
    {
        return _spinType switch
        {
            SpinType.Bronze => "bs",
            SpinType.Silver => "ss",
            SpinType.Gold => "gs",
            _ => "bs"
        };
    }

    private string GetTargetFolder()
    {
        var basePath = "Assets/08ScriptableObjects/SliceData";
        return _spinType switch
        {
            SpinType.Bronze => $"{basePath}/BronzeSpin",
            SpinType.Silver => $"{basePath}/SilverSpin",
            SpinType.Gold => $"{basePath}/GoldSpin",
            _ => $"{basePath}/BronzeSpin"
        };
    }

    private void OfferAddToConfig(WheelSliceData slice)
    {
        if (!EditorUtility.DisplayDialog("Add to Config?",
            $"Add '{slice.name}' to the {_spinType} wheel config's rewards list?", "Add", "Skip"))
            return;

        var configPath = _spinType switch
        {
            SpinType.Bronze => "Assets/08ScriptableObjects/ConfigData/cd_bronzespin.asset",
            SpinType.Silver => "Assets/08ScriptableObjects/ConfigData/cd_silverspin.asset",
            SpinType.Gold => "Assets/08ScriptableObjects/ConfigData/cd_goldspin.asset",
            _ => "Assets/08ScriptableObjects/ConfigData/cd_bronzespin.asset"
        };

        var config = AssetDatabase.LoadAssetAtPath<WheelConfigData>(configPath);
        if (config == null)
        {
            EditorUtility.DisplayDialog("Error", $"Config not found at:\n{configPath}", "OK");
            return;
        }

        var list = new System.Collections.Generic.List<WheelSliceData>();
        if (config.rewards != null)
            list.AddRange(config.rewards);

        list.Add(slice);
        config.rewards = list.ToArray();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", $"Added to {config.name}.rewards ({list.Count} total).", "OK");
        EditorGUIUtility.PingObject(config);
    }
}
