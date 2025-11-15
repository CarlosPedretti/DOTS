#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Utilities;

namespace Utilities
{
    public class CarlosToolKitInstallerWizard : EditorWindow
    {
        private enum WizardStep
        {
            Welcome,
            InstallDependencies,
            ImportToolkit
        }

        private WizardStep currentStep = WizardStep.Welcome;

        private Vector2 scroll;
        private AddRequest request;
        private bool isInstalling = false;
        private ListRequest listRequest;
        private static List<string> installedPackages = new();

        private const string DEPENDENCY_INSTALLER_FOLDER = "Assets/CarlosToolKitInstaller/DependenciesInstallers/DependencyToLoad";
        private static DependencyInstallsScriptable dependenciesScriptable;

        private static List<Dependency> dependencies;

        [MenuItem("CarlosTools/Instalador ToolKit (Wizard)", false, 2)]
        public static void ShowWindow()
        {
            GetWindow<CarlosToolKitInstallerWizard>("Instalador ToolKit");
        }

        private void OnEnable()
        {
            LoadDependencyInstallsAsset();

            listRequest = Client.List();
            EditorApplication.update += CheckInstalledPackages;
        }


        #region GUI Methods

        private void OnGUI()
        {
            GUILayout.Label("CarlosToolKit Installer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            switch (currentStep)
            {
                case WizardStep.Welcome:
                    DrawWelcomeStep();
                    break;
                case WizardStep.InstallDependencies:
                    DrawDependencyStep();
                    break;
                case WizardStep.ImportToolkit:
                    DrawImportStep();
                    break;
            }

            GUILayout.FlexibleSpace();
            DrawNavigationButtons();
        }

        private void DrawWelcomeStep()
        {
            GUILayout.Label("¡Bienvenido al instalador de CarlosToolKit!", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            GUILayout.Label("Este asistente te guiará a través de la instalación:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("1. Instalar dependencias\n2. Importar el contenido completo", EditorStyles.helpBox);
        }

        private void DrawDependencyStep()
        {
            GUILayout.Label("Paso 1: Instalar dependencias", EditorStyles.wordWrappedLabel);
            GUILayout.Space(5);
            GUILayout.Label("Se instalarán automáticamente desde el Registry o GitHub. Las demás se deben instalar manualmente.", EditorStyles.helpBox);

            var autoInstallable = dependencies.FindAll(CanBeInstalledAutomatically);
            var manualInstall = dependencies.FindAll(dep => !CanBeInstalledAutomatically(dep));

            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label("Dependencias automáticas", EditorStyles.boldLabel);
            foreach (var dep in autoInstallable)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"• {dep.DisplayName} ({dep.Version})");

                bool isAlreadyInstalled = IsInstalled(dep);

                GUI.enabled = !isInstalling && !isAlreadyInstalled;
                if (GUILayout.Button(isAlreadyInstalled ? "Ya instalado" : "Instalar"))
                {
                    InstallDependency(dep);
                }
                GUI.enabled = true;

                GUILayout.EndVertical();
            }

            GUILayout.Space(10);

            if (manualInstall.Count > 0)
            {
                GUILayout.Label("Instalación manual requerida", EditorStyles.boldLabel);
                foreach (var dep in manualInstall)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"• {dep.DisplayName} ({dep.Version})");
                    if (GUILayout.Button("Abrir enlace"))
                    {
                        Application.OpenURL(dep.Link);
                    }
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndScrollView();


        }

        private void DrawImportStep()
        {
            GUILayout.Label("Paso 2: Importar contenido del ToolKit", EditorStyles.wordWrappedLabel);
            GUILayout.Space(5);
            GUILayout.Label("Una vez finalizada la instalación de dependencias, puedes importar el resto del toolkit.", EditorStyles.helpBox);

            if (GUILayout.Button("Seleccionar e importar paquete .unitypackage"))
            {
                ImportCompleteContent();
            }
        }

        private void DrawNavigationButtons()
        {
            GUILayout.BeginHorizontal();

            GUI.enabled = currentStep != WizardStep.Welcome;
            if (GUILayout.Button("Anterior"))
            {
                currentStep--;
            }
            GUI.enabled = true;

            GUI.enabled = currentStep != WizardStep.ImportToolkit;
            if (GUILayout.Button("Siguiente"))
            {
                currentStep++;
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        private void ImportCompleteContent()
        {
            string path = EditorUtility.OpenFilePanel("Importar CarlosToolKit Completo", "", "unitypackage");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.ImportPackage(path, true);
                Debug.Log("CarlosToolKit full package imported.");
            }
        }

        #endregion


        #region Auxiliar Methods
        private void LoadDependencyInstallsAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:DependencyInstallsScriptable", new[] { DEPENDENCY_INSTALLER_FOLDER });

            if (guids.Length == 0)
            {
                Debug.LogError($"No se encontró ningún DependencyInstallsScriptable en la carpeta: {DEPENDENCY_INSTALLER_FOLDER}");
                dependencies = new List<Dependency>();
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            dependenciesScriptable = AssetDatabase.LoadAssetAtPath<DependencyInstallsScriptable>(path);

            if (dependenciesScriptable == null)
            {
                Debug.LogError($"Error al cargar el ScriptableObject en la ruta: {path}");
                dependencies = new List<Dependency>();
                return;
            }

            dependencies = dependenciesScriptable.dependencies;
        }

        private void CheckInstalledPackages()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    foreach (var package in listRequest.Result)
                    {
                        if (!installedPackages.Contains(package.name))
                        {
                            installedPackages.Add(package.name);
                        }
                    }
                }
                else if (listRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError("Failed to list installed packages: " + listRequest.Error.message);
                }

                EditorApplication.update -= CheckInstalledPackages;
            }
        }

        private bool IsInstalled(Dependency dep)
        {
            CheckInstalledPackages();

            return !string.IsNullOrEmpty(dep.PackageName) && installedPackages.Contains(dep.PackageName);
        }

        private bool CanBeInstalledAutomatically(Dependency dep)
        {
            return dep.InstallMethod == InstallMethod.UnityRegistry || dep.InstallMethod == InstallMethod.Git;
        }

        private void InstallDependency(Dependency dependency)
        {
            if (IsInstalled(dependency))
            {
                Debug.Log($"Skipped (already installed): {dependency.DisplayName}");
                return;
            }

            Debug.Log($"Installing: {dependency.DisplayName}");
            isInstalling = true;

            request = Client.Add(dependency.Source);
            EditorApplication.update += () =>
            {
                if (request.IsCompleted)
                {
                    if (request.Status == StatusCode.Success)
                    {
                        Debug.Log($"Installed: {request.Result.packageId}");
                        installedPackages.Add(dependency.PackageName);
                    }
                    else if (request.Status >= StatusCode.Failure)
                    {
                        Debug.LogError($"Failed to install package: {request.Error.message}");
                    }

                    isInstalling = false;
                    Repaint(); 
                }
            };
        }

        #endregion
    }
}

#endif
//private static readonly List<Dependency> dependencies = new()
//        {
//            new Dependency()
//            {
//                DisplayName = "Unity UI",
//                PackageName = "com.unity.ugui",
//                Source = "com.unity.ugui",
//                Version = "Latest",
//                Link = "https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/index.html",
//                InstallMethod = InstallMethod.UnityRegistry

//            },
//            new Dependency()
//            {
//                DisplayName = "DOTween",
//                PackageName = "DOTween",
//                Source = string.Empty,
//                Version = "Latest",
//                Link = "https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676",
//                InstallMethod = InstallMethod.AssetStore
//            },
//            new Dependency()
//            {
//                DisplayName = "NuGetForUnity",
//                PackageName = "com.github-glitchenzo.nugetforunity",
//                Source = "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity",
//                Version = "Latest",
//                Link = "https://github.com/GlitchEnzo/NuGetForUnity",
//                InstallMethod = InstallMethod.Git
//            },
//            new Dependency()
//            {
//                DisplayName = "New Input System",
//                PackageName = "com.unity.inputsystem",
//                Source = "com.unity.inputsystem",
//                Version = "1.7.0",
//                Link = "https://docs.unity3d.com/Packages/com.unity.inputsystem@latest",
//                InstallMethod = InstallMethod.UnityRegistry
//            },
//            new Dependency()
//            {
//                DisplayName = "Splines",
//                PackageName = "com.unity.splines",
//                Source = "com.unity.splines",
//                Version = "1.7.0",
//                Link = "https://docs.unity3d.com/Packages/com.unity.splines@1.0/manual/getting-started-with-splines.html",
//                InstallMethod = InstallMethod.UnityRegistry
//            },
//        };