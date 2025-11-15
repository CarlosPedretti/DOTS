using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Utilities
{
    public class CarlosToolKitExportToolsWindow : EditorWindow
    {
        [MenuItem("CarlosTools/Exportar ToolKit", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<CarlosToolKitExportToolsWindow>("Exportar ToolKit");
        }

        private void OnGUI()
        {
            GUILayout.Label("Exportar ToolKit como .unitypackage", EditorStyles.boldLabel);

            if (GUILayout.Button("Exportar ambos paquetes al Escritorio"))
            {
                ExportInstallerPackage();
                ExportContentPackage();
            }
        }

        private void ExportInstallerPackage()
        {

            string version = Application.version;
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string installerPath = Path.Combine(desktopPath, $"CarlosToolKitInstaller - v{version}.unitypackage");

            string[] installerFolders = new string[]
            {
                "Assets/CarlosToolKitInstaller"
            };

            AssetDatabase.ExportPackage(installerFolders, installerPath, ExportPackageOptions.Recurse);
            Debug.Log("CarlosToolKitInstaller exportado al escritorio.");
        }

        private void ExportContentPackage()
        {
            string version = Application.version;
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string contentPath = Path.Combine(desktopPath, $"CarlosToolKit - v{version}.unitypackage");

            string[] foldersToExport = new string[]
            {
                "Assets/CarlosToolKit",
                //"Assets/Plugins"
            };

            CreateVersionAsset(foldersToExport);

            AssetDatabase.ExportPackage(foldersToExport, contentPath, ExportPackageOptions.Recurse);
            Debug.Log("CarlosToolKit (contenido) exportado al escritorio.");
        }



        private void CreateVersionAsset(string[] foldersToExport)
        {
            string assetPath = "Assets/CarlosToolKit/Editor/CarlosToolKitVersion.asset";

            // Crear carpeta si no existe
            string folderPath = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            CarlosToolKitVersionInfo versionInfo = AssetDatabase.LoadAssetAtPath<CarlosToolKitVersionInfo>(assetPath);
            if (versionInfo == null)
            {
                versionInfo = ScriptableObject.CreateInstance<CarlosToolKitVersionInfo>();
                AssetDatabase.CreateAsset(versionInfo, assetPath);
            }

            versionInfo.version = Application.version;
            versionInfo.exportDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            versionInfo.hash = ComputeHash(foldersToExport);

            EditorUtility.SetDirty(versionInfo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private string ComputeHash(string[] folders)
        {
            using (MD5 md5 = MD5.Create())
            {
                foreach (var folder in folders)
                {
                    if (!Directory.Exists(folder)) continue;

                    foreach (var file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
                    {
                        byte[] content = File.ReadAllBytes(file);
                        md5.TransformBlock(content, 0, content.Length, null, 0);
                    }
                }
                md5.TransformFinalBlock(new byte[0], 0, 0);
                return System.BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
