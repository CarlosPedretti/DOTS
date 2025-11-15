using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MissingScriptsTool : EditorWindow
{
    private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
    private List<GameObject> prefabsWithMissingScripts = new List<GameObject>();
    private Vector2 scrollPos;

    [MenuItem("CarlosTools/Missing Scripts Tool", false, 21)]
    public static void ShowWindow()
    {
        GetWindow(typeof(MissingScriptsTool), false, "Missing Scripts Tool");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Scan Scene"))
        {
            FindInScene();
        }

        if (GUILayout.Button("Scan Prefabs"))
        {
            FindInPrefabs();
        }

        if (GUILayout.Button("Scan All"))
        {
            FindInScene();
            FindInPrefabs();
        }

        GUILayout.Space(5);
        GUILayout.Label("Scene Objects with Missing Scripts", EditorStyles.boldLabel);
        DrawList(objectsWithMissingScripts);

        GUILayout.Space(5);
        GUILayout.Label("Prefabs with Missing Scripts", EditorStyles.boldLabel);
        DrawList(prefabsWithMissingScripts);

        GUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Select All Missing Scripts (Scene + Prefabs)"))
        {
            List<Object> all = new List<Object>();
            all.AddRange(objectsWithMissingScripts);
            all.AddRange(prefabsWithMissingScripts);
            Selection.objects = all.ToArray();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Remove Missing Scripts from Selected"))
        {
            RemoveMissingScriptsFromSelectedObjects();
            FindInScene();
            FindInPrefabs();
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawList(List<GameObject> list)
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
        if (list.Count > 0)
        {
            foreach (GameObject go in list)
            {
                if (GUILayout.Button(go.name, GUILayout.ExpandWidth(true)))
                {
                    Selection.activeObject = go;
                    EditorGUIUtility.PingObject(go);
                }
            }
        }
        else
        {
            GUILayout.Label("No objects found.");
        }
        EditorGUILayout.EndScrollView();
    }

    private void FindInScene()
    {
        objectsWithMissingScripts.Clear();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && go.hideFlags == HideFlags.None)
            {
                Component[] components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        if (!objectsWithMissingScripts.Contains(go))
                            objectsWithMissingScripts.Add(go);
                        break;
                    }
                }
            }
        }

        Debug.Log($"[Scene] Found {objectsWithMissingScripts.Count} objects with missing scripts.");
    }

    private void FindInPrefabs()
    {
        prefabsWithMissingScripts.Clear();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool hasMissing = false;
            Transform[] allChildren = prefab.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allChildren)
            {
                Component[] components = t.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        if (!prefabsWithMissingScripts.Contains(prefab))
                            prefabsWithMissingScripts.Add(prefab);
                        hasMissing = true;
                        break;
                    }
                }
                if (hasMissing) break;
            }
        }

        Debug.Log($"[Prefabs] Found {prefabsWithMissingScripts.Count} prefabs with missing scripts.");
    }

    private void RemoveMissingScriptsFromSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.Log("No objects selected.");
            return;
        }

        foreach (GameObject go in selectedObjects)
        {
            Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            EditorUtility.SetDirty(go);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Removed missing scripts from selected objects.");
    }
}
