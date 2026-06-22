using UnityEngine;
using UnityEditor;
using System.Linq;

public class SceneHierarchyTool
{
    private const string MENU_PATH = "Tools/Wheel of Fortune/Setup Scene Hierarchy";
    private const string ROOT_NAME = "_Managers";

    [MenuItem(MENU_PATH)]
    public static void SetupHierarchy()
    {
        Undo.IncrementCurrentGroup();

        var root = GetOrCreateRoot();
        var folderM = GetOrCreateChild(root, "_Managers");
        var folderC = GetOrCreateChild(root, "_Controllers");
        var folderUI = GetOrCreateChild(root, "_UI");

        ReparentExisting("GameManager", folderM);
        ReparentExisting("AudioManager", folderM);
        ReparentExisting("AppBootstrap", folderM);

        CreateUIManagerHolder(folderUI, root);
        CreateWheelControllerHolder(folderC, root);

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

        Debug.Log($"[SceneHierarchyTool] Hierarchy setup complete. See _Managers GameObject.");
        EditorUtility.DisplayDialog("Scene Hierarchy Setup",
            "Done! Check the _Managers GameObject.\n\n" +
            "NEXT STEPS:\n" +
            "1. Open _Managers/_UI/UIManager\n" +
            "2. Assign 'Target Canvas' = Canvas in scene\n" +
            "3. Right-click old UIManager on Canvas → Copy Component\n" +
            "4. Select new UIManager → Paste Component as New\n" +
            "5. Delete old UIManager from Canvas\n\n" +
            "Same for WheelController:\n" +
            "1. Copy component from ui_panel_wheel\n" +
            "2. Paste on _Managers/_Controllers/WheelController\n" +
            "3. Delete old from ui_panel_wheel",
            "OK");
    }

    private static GameObject GetOrCreateRoot()
    {
        var existing = GameObject.Find(ROOT_NAME);
        if (existing != null)
        {
            var gs = existing.GetComponent<GameServices>();
            if (gs == null) existing.AddComponent<GameServices>();
            return existing;
        }
        var go = new GameObject(ROOT_NAME);
        go.AddComponent<GameServices>();
        Undo.RegisterCreatedObjectUndo(go, "Create _Managers");
        return go;
    }

    private static GameObject GetOrCreateChild(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        var child = new GameObject(name);
        child.transform.SetParent(parent.transform);
        Undo.RegisterCreatedObjectUndo(child, $"Create {name}");
        return child;
    }

    private static void ReparentExisting(string goName, GameObject newParent)
    {
        var go = GameObject.Find(goName);
        if (go == null) return;
        Undo.SetTransformParent(go.transform, newParent.transform, $"Reparent {goName}");
    }

    private static void CreateUIManagerHolder(GameObject folderUI, GameObject root)
    {
        var existing = folderUI.transform.Find("UIManager");
        if (existing != null) return;

        var go = new GameObject("UIManager");
        go.transform.SetParent(folderUI.transform);
        var ui = go.AddComponent<UIManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create UIManager holder");

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null) ui.targetCanvas = canvas;
    }

    private static void CreateWheelControllerHolder(GameObject folderC, GameObject root)
    {
        var existing = folderC.transform.Find("WheelController");
        if (existing != null) return;

        var go = new GameObject("WheelController");
        go.transform.SetParent(folderC.transform);
        go.AddComponent<WheelController>();
        Undo.RegisterCreatedObjectUndo(go, "Create WheelController holder");
    }
}
