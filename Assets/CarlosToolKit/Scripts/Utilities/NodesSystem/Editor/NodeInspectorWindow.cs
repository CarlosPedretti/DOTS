using UnityEditor;
using UnityEngine;

namespace Utilities
{
    public class NodeInspectorWindow : EditorWindow
    {
        private bool initialized = false;
        private NodeGraph targetNodeGraph;
        private INodeElement selectedNodeElement => targetNodeGraph?.SelectedNodeElement;
        private System.Action<INodeElement> onNodeSelectedCallback;

        [MenuItem("Window/Node Inspector (Debug)")]
        public static void OpenDebugWindow()
        {
            GetWindow<NodeInspectorWindow>("Node Inspector");
        }

        public static void ShowWindow(NodeGraph nodeGraph)
        {
            var window = GetWindow<NodeInspectorWindow>("Node Inspector");
            window.Initialize(nodeGraph);
            window.Show();
            FocusWindowIfItsOpen<SceneView>();
        }

        private void Initialize(NodeGraph nodeGraph)
        {
            targetNodeGraph = nodeGraph;

            if (onNodeSelectedCallback == null)
                onNodeSelectedCallback = (_) => Repaint();

            targetNodeGraph.OnNodeElementChanged -= onNodeSelectedCallback;
            targetNodeGraph.OnNodeElementChanged += onNodeSelectedCallback;

            if (!initialized)
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    Vector2 size = new Vector2(600, 300);
                    Rect sceneRect = sceneView.position;
                    position = new Rect(
                        sceneRect.x + sceneRect.width - size.x,
                        sceneRect.y + sceneRect.height - size.y,
                        size.x,
                        size.y
                    );
                }
                initialized = true;
            }
        }

        private void OnDisable()
        {
            if (targetNodeGraph != null && onNodeSelectedCallback != null)
                targetNodeGraph.OnNodeSelected -= onNodeSelectedCallback;
        }

        private void OnGUI()
        {
            if (selectedNodeElement == null)
            {
                EditorGUILayout.LabelField("No element selected.");
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("box");

            DisplayNodeGraphName();
            DisplayElementName();
            DisplayElementTransform(selectedNodeElement);
            DisplaySplineDataIfNode(selectedNodeElement);

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                if (selectedNodeElement is Node node)
                {
                    node.UpdateNode();
                    EditorUtility.SetDirty(node);
                }
            }
        }

        void DisplayNodeGraphName()
        {
            EditorGUILayout.LabelField("Node Graph Name", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(targetNodeGraph.name);
        }

        void DisplayElementName()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Element Name", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(selectedNodeElement.transform.name);
        }

        void DisplayElementTransform(INodeElement element)
        {
            if (element is MonoBehaviour behaviour)
            {
                Transform transform = behaviour.transform;

                EditorGUILayout.Space();
                Vector3 newPosition = EditorGUILayout.Vector3Field("Position", transform.position);
                if (newPosition != transform.position)
                {
                    Undo.RecordObject(transform, "Change Position");
                    transform.position = newPosition;
                }

                EditorGUILayout.Space();
                Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", transform.rotation.eulerAngles);
                if (newRotation != transform.rotation.eulerAngles)
                {
                    Undo.RecordObject(transform, "Change Rotation");
                    transform.rotation = Quaternion.Euler(newRotation);
                }
            }
        }

        void DisplaySplineDataIfNode(INodeElement element)
        {
            if (element is Node node)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("SplineType", EditorStyles.boldLabel);
                node.SplineType = (SplineType)EditorGUILayout.EnumPopup(node.SplineType);

                EditorGUILayout.Space();

                if (node.SplineType == SplineType.Bezier)
                {
                    Vector3 handleInPosition = EditorGUILayout.Vector3Field("In", node.handleIn.position);
                    if (handleInPosition != node.handleIn.position)
                    {
                        Undo.RecordObject(node.handleIn.transform, "Change handleIn Position");
                        node.handleIn.transform.position = handleInPosition;
                    }

                    EditorGUILayout.Space();

                    Vector3 handleOutPosition = EditorGUILayout.Vector3Field("Out", node.handleOut.position);
                    if (handleOutPosition != node.handleOut.position)
                    {
                        Undo.RecordObject(node.handleOut.transform, "Change handleOut Position");
                        node.handleOut.transform.position = handleOutPosition;
                    }
                }
            }
        }
    }
}
