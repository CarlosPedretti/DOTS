using UnityEngine;
using UnityEditor;

public class CarlosToolInfoWindow : EditorWindow
{
    private string toolkitVersion = "Desconocida";
    private string exportDate = "Desconocida";
    private string hash = "Desconocido";

    [MenuItem("CarlosTools/Info", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<CarlosToolInfoWindow>("CarlosToolKit");
    }

    private void OnEnable()
    {
        CarlosToolKitVersionInfo versionInfo = AssetDatabase.LoadAssetAtPath<CarlosToolKitVersionInfo>(
            "Assets/CarlosToolKit/Editor/CarlosToolKitVersion.asset");

        if (versionInfo != null)
        {
            toolkitVersion = versionInfo.version;
            exportDate = versionInfo.exportDate;
            hash = versionInfo.hash;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label($"CarlosToolKit Version: {toolkitVersion}", EditorStyles.boldLabel);
        GUILayout.Label($"Export Date: {exportDate}", EditorStyles.label);
        GUILayout.Label($"Hash: {hash}", EditorStyles.label);
    }
}
