using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DemoSettings))]
public class DemoSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        var style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, alignment = TextAnchor.MiddleCenter };
        GUILayout.Label("Player Settings Manager", style);
        GUILayout.Space(8);

        if (GUILayout.Button("Open Player Settings Window", GUILayout.Height(32)))
            PlayerSettingsWindow.ShowWindow();
    }
}
