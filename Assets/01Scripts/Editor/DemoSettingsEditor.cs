using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DemoSettings))]
public class DemoSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DemoSettings settings = (DemoSettings)target;
        EditorGUILayout.Space(10);

        if (GUILayout.Button("Clear All PlayerPrefs", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear PlayerPrefs","Are you sure you want to delete all saved data?", "Yes", "Cancel"))
            {
                settings.ClearAllPlayerPrefs();
            }
        }

        if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Reset to Defaults",$"Reset coins to {settings.startingCoins} and money to {settings.startingMoney}?", "Yes", "Cancel"))
            {
                settings.ResetDefaults();
            }
        }
    }
}
