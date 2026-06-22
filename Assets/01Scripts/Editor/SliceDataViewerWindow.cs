using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SliceDataViewerWindow : EditorWindow
{
    private enum SpinFilter { All, Bronze, Silver, Gold }
    private SpinFilter _filter = SpinFilter.All;
    private string _search = "";
    private Vector2 _scroll;
    private GUIStyle _rowStyle;

    private List<SliceEntry> _entries = new List<SliceEntry>();
    private bool _dirty = true;

    [MenuItem("Tools/Wheel of Fortune/View Slice Datas", priority = 111)]
    public static void ShowWindow()
    {
        var w = GetWindow<SliceDataViewerWindow>(false, "Slice Data Viewer");
        w.minSize = new Vector2(700, 400);
    }

    private class SliceEntry
    {
        public WheelSliceData data;
        public string label;
        public string folder;
        public string path;
    }

    private void OnEnable()
    {
        _dirty = true;
    }

    private void OnFocus()
    {
        _dirty = true;
    }

    private void Refresh()
    {
        _entries.Clear();
        var guids = AssetDatabase.FindAssets("t:WheelSliceData");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<WheelSliceData>(path);
            if (data == null) continue;

            var folder = Path.GetDirectoryName(path).Replace("\\", "/");
            var label = Path.GetFileNameWithoutExtension(path);
            _entries.Add(new SliceEntry { data = data, label = label, folder = folder, path = path });
        }
        _entries = _entries.OrderBy(e => e.folder).ThenBy(e => e.label).ToList();
        _dirty = false;
    }

    private void OnGUI()
    {
        if (_rowStyle == null)
        {
            _rowStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = false };
        }

        DrawToolbar();

        if (_dirty) Refresh();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        var filtered = GetFiltered();
        if (filtered.Count == 0)
        {
            EditorGUILayout.HelpBox("No slice data found. Create one via Tools → Wheel of Fortune → Slice Data Creator.", MessageType.Info);
        }
        else
        {
            DrawHeader();
            foreach (var entry in filtered)
                DrawEntry(entry);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(4);
        var bold = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleRight };
        EditorGUILayout.LabelField($"Total: {_entries.Count}  |  Showing: {filtered.Count}", bold);
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            _dirty = true;

        GUILayout.Space(6);

        _filter = (SpinFilter)EditorGUILayout.EnumPopup(_filter, EditorStyles.toolbarDropDown, GUILayout.Width(120));

        GUILayout.Space(6);
        GUILayout.Label("Search:", GUILayout.Width(48));
        _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarTextField, GUILayout.Width(180));

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Open Creator", EditorStyles.toolbarButton, GUILayout.Width(100)))
            SliceDataCreatorWindow.ShowWindow();

        GUILayout.EndHorizontal();
    }

    private List<SliceEntry> GetFiltered()
    {
        var q = _entries.AsEnumerable();

        q = _filter switch
        {
            SpinFilter.Bronze => q.Where(e => e.folder.Contains("BronzeSpin")),
            SpinFilter.Silver => q.Where(e => e.folder.Contains("SilverSpin")),
            SpinFilter.Gold => q.Where(e => e.folder.Contains("GoldSpin")),
            _ => q
        };

        if (!string.IsNullOrEmpty(_search))
        {
            var s = _search.ToLowerInvariant();
            q = q.Where(e => e.label.ToLowerInvariant().Contains(s)
                           || (e.data.rewardName != null && e.data.rewardName.ToLowerInvariant().Contains(s)));
        }

        return q.ToList();
    }

    private void DrawHeader()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", GUILayout.Width(44));
        GUILayout.Label("Spin", EditorStyles.boldLabel, GUILayout.Width(52));
        GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(120));
        GUILayout.Label("Reward", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.Label("Amount", EditorStyles.boldLabel, GUILayout.Width(60));
        GUILayout.Label("Bomb", EditorStyles.boldLabel, GUILayout.Width(42));
        GUILayout.Label("Path", EditorStyles.boldLabel);
        GUILayout.Label("", GUILayout.Width(120));
        GUILayout.EndHorizontal();
    }

    private void DrawEntry(SliceEntry entry)
    {
        var data = entry.data;
        var bg = new GUIStyle("CN EntryBackEven");

        GUILayout.BeginHorizontal(bg);

        // Icon
        if (data.icon != null)
            GUILayout.Label(new GUIContent(data.icon.texture), GUILayout.Width(40), GUILayout.Height(40));
        else
            GUILayout.Label("—", GUILayout.Width(40), GUILayout.Height(40));

        // Spin type badge
        var spinLabel = GetSpinLabel(entry.folder);
        var badgeColor = GetBadgeColor(spinLabel);
        var badgeStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = badgeColor }, fontStyle = FontStyle.Bold };
        GUILayout.Label(spinLabel, badgeStyle, GUILayout.Width(48));

        // Name
        GUILayout.Label(data.rewardName ?? "—", GUILayout.Width(116));

        // Full label
        GUILayout.Label(entry.label, GUILayout.Width(96));

        // Amount
        GUILayout.Label(data.rewardAmount.ToString("N0"), GUILayout.Width(56));

        // Bomb
        GUILayout.Label(data.isBomb ? "☠" : "", GUILayout.Width(38));

        // Path (truncated)
        var shortPath = entry.path.Length > 60 ? "..." + entry.path[^57..] : entry.path;
        GUILayout.Label(shortPath, _rowStyle);

        // Actions
        if (GUILayout.Button("Select", GUILayout.Width(48)))
        {
            EditorGUIUtility.PingObject(data);
            Selection.activeObject = data;
        }

        if (GUILayout.Button("Edit", GUILayout.Width(48)))
        {
            SliceDataEditorWindow.Open(entry.path);
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("Delete", GUILayout.Width(48)))
        {
            if (EditorUtility.DisplayDialog("Delete Slice Data",
                $"Delete '{entry.label}'?", "Delete", "Cancel"))
            {
                AssetDatabase.DeleteAsset(entry.path);
                _dirty = true;
            }
        }

        GUILayout.EndHorizontal();
    }

    private static string GetSpinLabel(string folder)
    {
        if (folder.Contains("BronzeSpin")) return "BRONZE";
        if (folder.Contains("SilverSpin")) return "SILVER";
        if (folder.Contains("GoldSpin")) return "GOLD";
        return "—";
    }

    private static Color GetBadgeColor(string spin)
    {
        return spin switch
        {
            "BRONZE" => new Color(0.58f, 0.34f, 0.11f),
            "SILVER" => new Color(0.55f, 0.55f, 0.60f),
            "GOLD" => new Color(1f, 0.76f, 0.055f),
            _ => Color.gray
        };
    }
}
