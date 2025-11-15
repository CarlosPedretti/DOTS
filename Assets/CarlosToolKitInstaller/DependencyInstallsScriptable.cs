using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    [CreateAssetMenu(fileName = "DependencyInstallsScriptable", menuName = "Scriptable Objects/DependencyInstallsScriptable")]
    public class DependencyInstallsScriptable : ScriptableObject
    {
        public List<Dependency> dependencies = new();
    }

    [System.Serializable]
    public class Dependency
    {
        public string DisplayName;
        public string PackageName;
        public string Source;
        public string Version;
        public string Link;
        public InstallMethod InstallMethod;
    }

    public enum InstallMethod
    {
        UnityRegistry,
        Git,
        AssetStore,
        External,
        Manual
    }
}

