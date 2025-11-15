using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    [ExecuteInEditMode]
    public class NodeGraph : MonoBehaviour
    {

        [SerializeField] public NodeConfiguration Config;

        public VisualSettings visualSettings { get { return Config.VisualSettings; } }
        public Bindings KeyBindings { get { return Config.KeyBindings; } }

        public int InitialNodeIndex
        {
            get
            {
                if (Config.InitialNodeIndex < 0 || Config.InitialNodeIndex >= nodes.Count)
                {
                    return 0;
                }

                return Config.InitialNodeIndex;
            }
        }


        public List<Node> Nodes { get { return nodes; } }
        [SerializeField] private List<Node> nodes = new List<Node>();

        public List<NodeConnection> Connections { get { return connections; } }
        [SerializeField] private List<NodeConnection> connections = new List<NodeConnection>();

        [SerializeField] private Dictionary<int, Node> nodesByID = new Dictionary<int, Node>();

        [HideInInspector][SerializeField] private INodeElement selectedNodeElement;

        public INodeElement SelectedNodeElement => selectedNodeElement;

        [HideInInspector] public UnityEvent OnConfigurationChanged;

        public event Action<Node> OnNodeCreated;
        public event Action<Node> OnNodeRemoved;
        public event Action<Node> OnNodeSelected;
        public event Action<INodeElement> OnNodeElementChanged;

        public Node CurrentSelectedNode
        {
            get
            {
                if (selectedNodeElement is Node)
                {
                    return selectedNodeElement as Node;
                }

                return null;
            }
        }


        #region Bake

        private void Start()
        {
            RefreshIDLookup();
        }

        public virtual void ClearNodes()
        {
            if (nodes == null || nodes.Count == 0)
            {
                UnityEngine.Debug.LogWarning("There are no nodes to clear.");
                return;
            }

#if UNITY_EDITOR
            var nodesCopy = new List<Node>(nodes);

            foreach (var node in nodesCopy)
            {
                if (node != null)
                {
                    Undo.DestroyObjectImmediate(node.gameObject);
                }
            }
#endif

            nodes.Clear();
            connections.Clear();
        }

        public virtual void BakeGraph()
        {
            if (nodes == null || nodes.Count == 0)
            {
                UnityEngine.Debug.LogWarning("There are no nodes to bake.");
                return;
            }

            if (Config.InitialNodeIndex < 0 || Config.InitialNodeIndex >= nodes.Count)
            {
                UnityEngine.Debug.LogWarning($"Invalid InitialKnotID: {Config.InitialNodeIndex}. Falling back to 0.");
                Config.InitialNodeIndex = 0;
            }

            switch (Config.BakeMode)
            {
                case PathNodeBakeMode.None:
                    BakeToNormal();
                    break;

                case PathNodeBakeMode.BFS:
                    BakeBFS();
                    break;
                case PathNodeBakeMode.DFS:
                    BakeDFS();
                    break;
            }

            UpdateAllAdjacentNodes();

            UnityEngine.Debug.Log($"Bake using {Config.BakeMode} mode completed.");
        }

        private void BakeToNormal()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].SetID(i);
            }
        }

        private void BakeBFS()
        {
            Queue<Node> queue = new Queue<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            int id = 0;

            var startNode = nodes[InitialNodeIndex];
            queue.Enqueue(startNode);
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                node.SetID(id++);
#if UNITY_EDITOR
                EditorUtility.SetDirty(node);
#endif

                foreach (var neighbor in GetConnectedNeighbors(node))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        private void BakeDFS()
        {
            HashSet<Node> visited = new HashSet<Node>();
            int id = 0;

            void DFS(Node node)
            {
                if (visited.Contains(node)) return;

                visited.Add(node);
                node.SetID(id++);
#if UNITY_EDITOR
                EditorUtility.SetDirty(node);
#endif

                foreach (var neighbor in GetConnectedNeighbors(node))
                {
                    DFS(neighbor);
                }
            }

            DFS(nodes[InitialNodeIndex]);
        }

        private IEnumerable<Node> GetConnectedNeighbors(Node node)
        {
            foreach (var conn in connections)
            {
                if (conn.from == node && conn.to != null)
                    yield return conn.to;
                else if (conn.to == node && conn.from != null)
                    yield return conn.from;
            }
        }

        private void UpdateAllAdjacentNodes()
        {
            Dictionary<Node, List<Node>> adjacentsMap = new Dictionary<Node, List<Node>>();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                adjacentsMap[node] = new List<Node>();
                node.SetIndex(i);
            }


            foreach (var conn in connections)
            {
                if (conn.from != null && conn.to != null)
                {
                    if (!adjacentsMap[conn.from].Contains(conn.to))
                        adjacentsMap[conn.from].Add(conn.to);

                    if (!adjacentsMap[conn.to].Contains(conn.from))
                        adjacentsMap[conn.to].Add(conn.from);
                }
            }
#if UNITY_EDITOR
            foreach (var pair in adjacentsMap)
            {
                pair.Key.SetAdjacents(pair.Value);
                EditorUtility.SetDirty(pair.Key);
            }
#endif

            Debug("Updated adjacent nodes for all PathNodes.");
        }

        private void RefreshIDLookup()
        {
            nodesByID.Clear();
            foreach (var node in nodes)
            {
                if (node != null)
                {
                    nodesByID[node.ID] = node;
                }
            }
        }


        #endregion

        #region Node Logic

        public Node CreateNode(Vector3 position)
        {
            GameObject newNodeGO = new GameObject("PathNode");
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(newNodeGO, "Create PathNode");
#endif

            newNodeGO.transform.position = position;
            newNodeGO.transform.parent = this.transform;

            var newNode = newNodeGO.AddComponent<Node>();
#if UNITY_EDITOR
            Undo.RecordObject(this, "Add Node");
#endif
            nodes.Add(newNode);

            int index = nodes.Count - 1;
            newNode.Init(this, index);

            OnNodeCreated?.Invoke(newNode);

            return newNode;
        }
        public void RemoveNode(Node node)
        {
            if (node == null) return;

            int removedConnections = connections.RemoveAll(c => c.from == node || c.to == node);

            if (nodes.Remove(node))
            {
                Debug($"Removed node '{node.name}' and {removedConnections} related connections.");
            }
            else
            {
                Debug($"Node '{node.name}' was not found in the nodes list.");
            }

            OnNodeRemoved?.Invoke(node);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        public void SetSelectedNodeElement(INodeElement nodeElement)
        {
            selectedNodeElement = nodeElement;

            if (selectedNodeElement is Node)
            {
                OnNodeSelected?.Invoke(selectedNodeElement as Node);
            }

            OnNodeElementChanged?.Invoke(selectedNodeElement);
        }
        public void ConnectNode(Node node)
        {
            if (CurrentSelectedNode is Node currentPathNode)
            {
                var conn = new NodeConnection
                {
                    from = currentPathNode,
                    to = node,
                };

                connections.Add(conn);
            }

            SetSelectedNodeElement(node);
        }

        public bool TryCreateConnection(Node from, Node to, float tension = 0.5f)
        {
            if (from == null || to == null || from == to)
                return false;

            bool alreadyConnected = connections.Exists(c =>
                (c.from == from && c.to == to) || (c.from == to && c.to == from));

            if (alreadyConnected)
                return false;

            var conn = new NodeConnection
            {
                from = from,
                to = to,
            };

            connections.Add(conn);

            AdjustHandlesAlongDirection(from, to);


            return true;
        }

        private void AdjustHandlesAlongDirection(Node a, Node b)
        {
            Vector3 direction = (b.transform.position - a.transform.position).normalized;
            float handleDistance = visualSettings.nodeRadius + Config.InitialHandleSeparation;

            if (a.handleOut != null)
                a.handleOut.position = a.transform.position + direction * handleDistance;

            if (b.handleIn != null)
                b.handleIn.position = b.transform.position - direction * handleDistance;
        }

        public Node GetNodeByID(int id)
        {
            nodesByID.TryGetValue(id, out Node result);
            return result;
        }

        #endregion


        #region GUI
        private void OnDrawGizmos()
        {
            if (nodes == null || connections == null || !Config.ShowVisuals || !visualSettings.drawnConnections) return;

            foreach (var conn in connections)
            {
                if (conn.from == null || conn.to == null) continue;

                Node fromNode = conn.from;
                Node toNode = conn.to;

                bool useBezier = fromNode.SplineType == SplineType.Bezier || toNode.SplineType == SplineType.Bezier;

                if (useBezier)
                {
                    Vector3 p0 = fromNode.transform.position;
                    Vector3 p3 = toNode.transform.position;

                    Vector3 p1 = fromNode.handleOut != null ? fromNode.handleOut.position : p0;
                    Vector3 p2 = toNode.handleIn != null ? toNode.handleIn.position : p3;

                    DrawBezier(p0, p1, p2, p3);
                }
                else
                {
                    DrawStraightLine(fromNode.transform.position, toNode.transform.position);
                }

                if (visualSettings.drawConnectionPoint)
                {
                    Gizmos.DrawSphere(fromNode.transform.position, visualSettings.nodeRadius * visualSettings.connectionPointRadius);
                    Gizmos.DrawSphere(toNode.transform.position, visualSettings.nodeRadius * visualSettings.connectionPointRadius);
                }
            }
        }

        private void DrawStraightLine(Vector3 start, Vector3 end)
        {
#if UNITY_EDITOR
            Handles.color = visualSettings.curveColor;
            Handles.DrawAAPolyLine(visualSettings.curveGrosor, start, end);
#endif
        }

        private void DrawBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
#if UNITY_EDITOR
            Handles.color = visualSettings.curveColor;

            Vector3[] bezierPoints = new Vector3[21];
            for (int i = 0; i <= 20; i++)
            {
                float t = i / 20f;
                bezierPoints[i] = Mathf.Pow(1 - t, 3) * p0 +
                                  3 * Mathf.Pow(1 - t, 2) * t * p1 +
                                  3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                                  Mathf.Pow(t, 3) * p3;
            }

            Handles.DrawAAPolyLine(visualSettings.curveGrosor, bezierPoints);
#endif
        }

        public void NotifyConfigurationChanged()
        {
            OnConfigurationChanged?.Invoke();
        }

        #endregion


        private void Debug(object message)
        {
            if (!Config.ShowDebugs) return;

            UnityEngine.Debug.Log(message);
        }

    }

    public enum PathNodeBakeMode
    {
        None,
        DFS,
        BFS,
    }
}
