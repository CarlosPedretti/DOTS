using UnityEditor;
using UnityEngine;

namespace Utilities
{
    [CustomEditor(typeof(NodeGraph))]
    public class NodeGraphEditor : Editor
    {
        private NodeGraph graph;

        private SerializedProperty configProp;
        private SerializedProperty nodesProp;
        private SerializedProperty connectionsProp;

        public virtual void OnEnable()
        {
            graph = (NodeGraph)target;

            configProp = serializedObject.FindProperty("Config");

            //Lists
            nodesProp = serializedObject.FindProperty("nodes");
            connectionsProp = serializedObject.FindProperty("connections");
        }

        private void OnSceneGUI()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawConfigurationSection();
            DrawGraphListsSection();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                graph.NotifyConfigurationChanged(); // Notify config changed
                EditorUtility.SetDirty(graph);
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();
            DrawActionButtons();
        }

        private void DrawConfigurationSection()
        {
            EditorGUILayout.LabelField("Node Configuration", EditorStyles.boldLabel);
            if (configProp != null)
            {
                EditorGUILayout.PropertyField(configProp, true);
            }
            else
            {
                EditorGUILayout.HelpBox("The configProp could not be found", MessageType.Warning);
            }

            EditorGUILayout.Space();
        }

        private void DrawGraphListsSection()
        {
            EditorGUILayout.LabelField("Graph Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(nodesProp, true);
            EditorGUILayout.PropertyField(connectionsProp, true);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("Graph Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Bake Graph"))
            {
                graph.BakeGraph();
                EditorUtility.SetDirty(graph);
            }

            if (GUILayout.Button("Clear Nodes"))
            {
                graph.ClearNodes();
                EditorUtility.SetDirty(graph);
            }
        }
    }
}
