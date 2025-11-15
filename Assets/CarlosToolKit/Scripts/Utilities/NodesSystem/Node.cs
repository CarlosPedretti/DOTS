using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Utilities
{
    [ExecuteInEditMode]
    public class Node : MonoBehaviour, INodeElement
    {

        [HideInInspector][SerializeField] private int id = -1;
        public int ID { get { return id; } }


        [HideInInspector][SerializeField] private int index = -1;
        public int Index { get { return index; } }

        [SerializeField] private List<Node> adjacentNodes = new List<Node>();
        public List<Node> AdjacentNodes { get { return adjacentNodes; } }

        public SplineType SplineType;
        public Transform handleIn;
        public Transform handleOut;

        [HideInInspector][SerializeField] private NodeGraph nodeGraph;
        private bool isSelected;

        [HideInInspector][SerializeField] private SphereCollider sphereCollider;


        void OnValidate()
        {
#if UNITY_EDITOR
            if (nodeGraph != null) nodeGraph.OnConfigurationChanged.AddListener(OnConfigurationChangedEvent);
#endif
        }

        public void Init(NodeGraph pathGraph, int index)
        {
            this.nodeGraph = pathGraph;
            this.index = index;
            gameObject.name = pathGraph.Config.NodeNameFormat + index;

            sphereCollider = this.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = pathGraph.visualSettings.nodeRadius;

            InitHandles();
#if UNITY_EDITOR
            nodeGraph.OnConfigurationChanged.AddListener(OnConfigurationChangedEvent);
#endif
        }

        private void InitHandles()
        {
            float handleDistance = nodeGraph.visualSettings.nodeRadius + nodeGraph.Config.InitialHandleSeparation;

            Vector3 direction = Vector3.right;

            if (adjacentNodes != null && adjacentNodes.Count > 0)
            {
                direction = (adjacentNodes[0].transform.position - transform.position).normalized;
            }

            // Create handleIn
            if (!handleIn)
            {
                GameObject hin = new GameObject("Handle_In");
                NodeHandler nodeHandler = hin.AddComponent<NodeHandler>();
                nodeHandler.Init(nodeGraph, this, NodeHandlerType.In);
                hin.transform.parent = transform;
                hin.transform.position = transform.position - direction * handleDistance;
                handleIn = hin.transform;
            }

            // Create handleOut
            if (!handleOut)
            {
                GameObject hout = new GameObject("Handle_Out");
                NodeHandler nodeHandler = hout.AddComponent<NodeHandler>();
                nodeHandler.Init(nodeGraph, this, NodeHandlerType.Out);
                hout.transform.parent = transform;
                hout.transform.position = transform.position + direction * handleDistance;
                handleOut = hout.transform;
            }

            UpdateHandlersEvent();
        }

        public List<Node> GetAdjacents()
        {
            return adjacentNodes;
        }

        private void OnDestroy()
        {
            nodeGraph.OnConfigurationChanged.RemoveListener(OnConfigurationChangedEvent);

            if (Application.isPlaying) return;

            var graph = GetComponentInParent<NodeGraph>();

            if (graph != null)
            {
                graph.RemoveNode(this);
            }
        }




        #region Settters Methods
        public void SetID(int id)
        {
            this.id = id;
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void SetSplineType(SplineType type)
        {
            SplineType = type;
        }

        public void SetAdjacents(List<Node> newAdjacents)
        {
            adjacentNodes = newAdjacents;
        }
        #endregion

        #region GUI

        public void UpdateNode()
        {
#if UNITY_EDITOR
            UpdateColliderEvent();
            UpdateHandlersEvent();
#endif
        }


        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (nodeGraph == null || !nodeGraph.Config.ShowVisuals) return;

            UpdateNodeForm();
            UpdateNodeLabel();
#endif
        }

        void OnConfigurationChangedEvent()
        {
#if UNITY_EDITOR
            UpdateColliderEvent();
#endif
        }

        void UpdateNodeForm()
        {
#if UNITY_EDITOR
            isSelected = nodeGraph.CurrentSelectedNode == this;

            if (isSelected)
            {
                Gizmos.color = nodeGraph.visualSettings.selectedNodeColor;
            }
            else if (nodeGraph.InitialNodeIndex == index)
            {
                Gizmos.color = nodeGraph.visualSettings.initialNodeColor;
            }
            else
            {
                Gizmos.color = nodeGraph.visualSettings.nodeColor;
            }


            Gizmos.DrawSphere(this.transform.position, nodeGraph.visualSettings.nodeRadius);

            float currentRadius = nodeGraph.visualSettings.nodeRadius;
#endif
        }
        void UpdateNodeLabel()
        {
#if UNITY_EDITOR
            if (!nodeGraph.visualSettings.showLabel) return;

            Vector3 basePosition = this.transform.position;
            Camera sceneCamera = SceneView.lastActiveSceneView?.camera;

            if (sceneCamera == null) return;

            float distance = Vector3.Distance(sceneCamera.transform.position, basePosition);

            if (distance > nodeGraph.visualSettings.maxLabelDistance) return;

            // Scale verticaOffset based on dinstance
            float baseHeight = nodeGraph.visualSettings.labelHeight + nodeGraph.visualSettings.nodeRadius;
            float scaledVerticalOffset = nodeGraph.visualSettings.verticalOffset * Mathf.Clamp(distance * 0.05f, 1f, 5f);

            if (nodeGraph.visualSettings.showID)
            {
                GUIStyle idStyle = new GUIStyle();
                idStyle.normal.textColor = nodeGraph.visualSettings.IDColor;
                idStyle.fontSize = nodeGraph.visualSettings.fontSize;
                Handles.Label(basePosition + Vector3.up * (baseHeight + scaledVerticalOffset), $"ID: {ID}", idStyle);
            }

            if (nodeGraph.visualSettings.showIndex)
            {
                GUIStyle indexStyle = new GUIStyle();
                indexStyle.normal.textColor = nodeGraph.visualSettings.IndexColor;
                indexStyle.fontSize = nodeGraph.visualSettings.fontSize;
                Handles.Label(basePosition + Vector3.up * baseHeight, $"Index: {index}", indexStyle);
            }
#endif
        }
        void UpdateColliderEvent()
        {
#if UNITY_EDITOR
            sphereCollider.radius = nodeGraph.visualSettings.nodeRadius;
#endif
        }
        void UpdateHandlersEvent()
        {
#if UNITY_EDITOR
            if (SplineType == SplineType.Linear)
            {
                handleIn.gameObject.SetActive(false);
                handleOut.gameObject.SetActive(false);
            }
            else
            {
                handleIn.gameObject.SetActive(true);
                handleOut.gameObject.SetActive(true);
            }
#endif
        }
#endregion
    }

    [System.Serializable]
    public class NodeConnection
    {
        public Node from;
        public Node to;

    }

    public enum SplineType 
    {
        Linear,
        Bezier
    }
}
