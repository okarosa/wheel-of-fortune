using UnityEngine;
using UnityEditor;
using System.IO;

public class SliceDataCreatorWindow : EditorWindow
{
    private enum SpinType { Bronze, Silver, Gold }
    private SpinType _spinType = SpinType.Bronze;
    private string _rewardName = "coin";
    private int _amount = 10;
    private Sprite _icon;
    private bool _isBomb;
    private Color _sliceColor = Color.white;
    private Vector2 _scroll;

    [MenuItem("Tools/Wheel of Fortune/Slice Data Creator", priority = 110)]
    public static void ShowWindow()
    {
        var w = GetWindow<SliceDataCreatorWindow>(true, "Slice Data Creator", true);
        w.minSize = new Vector2(380, 320);
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

        _isBomb = EditorGUILayout.Toggle("Is Bomb", _isBomb);

        if (!_isBomb)
        {
            _rewardName = EditorGUILayout.TextField("Reward Name", _rewardName).ToLowerInvariant().Trim();
            _amount = EditorGUILayout.IntField("Amount", Mathf.Max(1, _amount));
            _icon = (Sprite)EditorGUILayout.ObjectField("Icon", _icon, typeof(Sprite), false);
        }
        else
        {
            GUI.enabled = false;
            _rewardName = "bomb";
            _amount = 0;
            _icon = null;
            EditorGUILayout.TextField("Reward Name", "bomb");
            EditorGUILayout.IntField("Amount", 0);
            EditorGUILayout.ObjectField("Icon", _icon, typeof(Sprite), false);
            GUI.enabled = true;
        }

        _sliceColor = EditorGUILayout.ColorField("Slice Color", _sliceColor);

        GUILayout.Space(10);
        DrawPreview();
        GUILayout.Space(10);

        GUI.enabled = CanCreate();
        if (GUILayout.Button("Create Slice Data", GUILayout.Height(36)))
            CreateSliceData();
        GUI.enabled = true;

        EditorGUILayout.EndScrollView();
    }

    private void DrawPreview()
    {
        var prefix = GetPrefix();
        var namePart = _isBomb ? "bomb" : $"{_rewardName}_{_amount}";
        var fileName = $"{prefix}_{namePart}.asset";
        var folder = GetTargetFolder();

        var boxStyle = new GUIStyle(EditorStyles.helpBox);
        GUILayout.BeginVertical(boxStyle);
        var bold = new GUIStyle(EditorStyles.boldLabel);
        GUILayout.Label("Preview", bold);
        GUILayout.Label($"  File: {fileName}");
        GUILayout.Label($"  Path: {folder}/{fileName}");
        if (!_isBomb && _icon != null)
        {
            var rt = GUILayoutUtility.GetRect(48, 48);
            GUI.DrawTexture(rt, _icon.texture, ScaleMode.ScaleToFit);
        }
        GUILayout.EndVertical();
    }

    private bool CanCreate()
    {
        if (!_isBomb && (string.IsNullOrEmpty(_rewardName) || _amount <= 0))
            return false;
        return true;
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
        var namePart = _isBomb ? "bomb" : $"{_rewardName}_{_amount}";
        var assetName = $"{prefix}_{namePart}";
        var path = $"{folder}/{assetName}.asset";

        if (File.Exists(path))
        {
            if (!EditorUtility.DisplayDialog("Overwrite?",
                $"'{assetName}.asset' already exists.\nOverwrite it?", "Overwrite", "Cancel"))
                return;
            AssetDatabase.DeleteAsset(path);
        }

        var data = CreateInstance<WheelSliceData>();
        data.rewardName = _isBomb ? "bomb" : _rewardName;
        data.rewardAmount = _isBomb ? 0 : _amount;
        data.isBomb = _isBomb;
        data.icon = _isBomb ? null : _icon;
        data.sliceColor = _sliceColor;
        data.name = assetName;

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(data);
        Selection.activeObject = data;

        Debug.Log($"[SliceDataCreator] Created: {path}");

        if (!_isBomb)
        {
            _amount++;
            Repaint();
        }
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
}
