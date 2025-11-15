using UnityEngine;
using System;
using Utilities;
namespace Utilities
{
    [System.Serializable]
    public class NodeConfiguration
    {

        [Header("Editor Settings")]
        public bool ShowDebugs;
        public bool ShowVisuals = true;
        public bool AutoBake = true;
        public Bindings KeyBindings;
        public VisualSettings VisualSettings = new VisualSettings();

        [Header("Configuration")]
        public string NodeNameFormat = "PathNode_Index_";
        public float InitialHandleSeparation = 5f;
        public PathNodeBakeMode BakeMode;
        public int InitialNodeIndex;
    }

    [System.Serializable]
    public class VisualSettings
    {
        [Header("Node Appearance")]
        public float nodeRadius = 3f;
        public Color nodeColor = Color.white;
        public Color initialNodeColor = Color.cyan;
        public Color selectedNodeColor = Color.red;

        [Header("Connection Appearance")]
        public bool drawnConnections = true;
        public Color curveColor = Color.white;
        public float curveGrosor = 1f;
        public Color handleInColor = Color.blue;
        public Color handleOutColor = Color.yellow;
        public bool drawConnectionPoint = false;
        public float connectionPointRadius = 0.20f;

        [Header("Node Label")]
        public bool showLabel = true;
        [Range(0, 1000)]
        public float maxLabelDistance = 500f;
        public int fontSize = 20;
        public Color fontColor = Color.white;
        public float labelHeight = 5;
        public float verticalOffset = 1f;

        public bool showID = true;
        public Color IDColor = Color.cyan;

        public bool showIndex = true;
        public Color IndexColor = Color.green;
    }

    [System.Serializable]
    public class Bindings
    {
        public KeyCode DeleteKey = KeyCode.Delete;
        public KeyCode MoveKey = KeyCode.Space;
        public KeyCode ConnectKey = KeyCode.LeftShift;
        public KeyCode EscKey = KeyCode.Escape;
    }
}
