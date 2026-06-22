using UnityEngine;
using UnityEditor;

public class SliceDataEditorWindow : EditorWindow
{
    private enum RewardNameEnum { None, coin, money, armor, knife, rifle, pistol, submachine, shotgun, sniper, @case }

    private WheelSliceData _target;
    private string _assetPath;
    private RewardNameEnum _selectedEnum;
    private string _customNameValue;
    private int _amount;
    private Sprite _icon;
    private Vector2 _scroll;
    private bool _dirty;

    public static void Open(string assetPath)
    {
        var data = AssetDatabase.LoadAssetAtPath<WheelSliceData>(assetPath);
        if (data == null) return;

        var w = CreateInstance<SliceDataEditorWindow>();
        w.titleContent = new GUIContent($"Edit: {data.name}");
        w._target = data;
        w._assetPath = assetPath;
        w.LoadFromData();
        w.ShowUtility();
    }

    private void LoadFromData()
    {
        if (_target == null) return;

        _amount = _target.rewardAmount;
        _icon = _target.icon;

        var name = _target.rewardName ?? "";
        if (System.Enum.TryParse<RewardNameEnum>(name, true, out var parsed) && parsed != RewardNameEnum.None)
        {
            _selectedEnum = parsed;
            _customNameValue = "";
        }
        else
        {
            _selectedEnum = RewardNameEnum.None;
            _customNameValue = name;
        }
    }

    private string ResolveName()
    {
        if (_selectedEnum == RewardNameEnum.None)
            return _customNameValue.ToLowerInvariant().Trim();
        return _selectedEnum.ToString().ToLowerInvariant();
    }

    private void OnGUI()
    {
        if (_target == null) { Close(); return; }

        var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        GUILayout.Space(10);
        GUILayout.Label($"Editing: {_target.name}", titleStyle);
        GUILayout.Space(15);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUI.BeginChangeCheck();

        _selectedEnum = (RewardNameEnum)EditorGUILayout.EnumPopup("Reward Name", _selectedEnum);

        if (_selectedEnum == RewardNameEnum.None)
            _customNameValue = EditorGUILayout.TextField("Custom Name", _customNameValue);

        _amount = EditorGUILayout.IntField("Amount", Mathf.Max(1, _amount));
        _icon = (Sprite)EditorGUILayout.ObjectField("Icon", _icon, typeof(Sprite), false);

        if (EditorGUI.EndChangeCheck())
            _dirty = true;

        GUILayout.Space(15);

        var nameOk = !string.IsNullOrEmpty(ResolveName());
        GUI.enabled = nameOk && _dirty;
        if (GUILayout.Button("Save Changes", GUILayout.Height(36)))
            Save();
        GUI.enabled = true;

        if (!nameOk)
            EditorGUILayout.HelpBox("Reward name is required.", MessageType.Warning);

        EditorGUILayout.EndScrollView();
    }

    private void Save()
    {
        if (_target == null) return;

        var newName = ResolveName();
        _target.rewardName = newName;
        _target.rewardAmount = _amount;
        _target.icon = _icon;

        // Rename asset file
        var dir = System.IO.Path.GetDirectoryName(_assetPath).Replace("\\", "/");
        var prefix = "";
        if (dir.Contains("BronzeSpin")) prefix = "bs";
        else if (dir.Contains("SilverSpin")) prefix = "ss";
        else if (dir.Contains("GoldSpin")) prefix = "gs";

        var newAssetName = $"{prefix}_{newName}_{_amount}";
        var newPath = $"{dir}/{newAssetName}.asset";

        if (newPath != _assetPath)
        {
            if (System.IO.File.Exists(newPath) && newPath != _assetPath)
            {
                if (!EditorUtility.DisplayDialog("Overwrite?",
                    $"'{newAssetName}.asset' already exists.\nOverwrite it?", "Overwrite", "Cancel"))
                    return;
                AssetDatabase.DeleteAsset(newPath);
            }

            AssetDatabase.RenameAsset(_assetPath, newAssetName);
            _assetPath = newPath;
            _target.name = newAssetName;
        }

        EditorUtility.SetDirty(_target);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _dirty = false;
        titleContent = new GUIContent($"Edit: {_target.name}");
        Debug.Log($"[SliceDataEditor] Saved: {_assetPath}");
    }
}
