using QFSW.QC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class NodeNavigator : MonoBehaviour
    {
        [Header("Dependency")]
        [SerializeField] private NodeGraph nodeGraph;

        [Header("Configuration")]
        [SerializeField] bool showDebugs;
        [SerializeField] private Node initialNode;
        [SerializeField] private NodeTraversalDirection direction = NodeTraversalDirection.Forward;
        [SerializeField] private TraversalEndBehavior endBehavior = TraversalEndBehavior.LoopToEdge;

        [Header("Events")]
        public UnityEvent<Node> OnNodeChanged;
        public UnityEvent<Node> OnTraversalStopped;

        private Node currentNode;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (!IsGraphValid())
            {
                Debug.LogWarning("NodeGraph is not assigned or contains no nodes.");
                return;
            }

            currentNode = (initialNode != null && nodeGraph.Nodes.Contains(initialNode))
                ? initialNode
                : nodeGraph.Nodes[0];

            OnNodeChanged?.Invoke(currentNode);
        }


        #region Public Methods


        /// <summary>
        /// Returns the world position of the next node based on the current traversal direction
        /// and configured end behavior. Returns null if no valid next node is found.
        /// </summary>
        /// <returns>
        /// A <see cref="Vector3"/> representing the position of the next node, or null if unavailable.
        /// </returns>
        public Vector3? GetNextNodePosition()
        {
            Node nextNode = GetNextNodeByID(direction, endBehavior);
            return nextNode != null ? nextNode.transform.position : (Vector3?)null;
        }


        /// <summary>
        /// Returns a list of world positions forming the path from a given starting point
        /// to the next node in the graph. If a Bézier curve is defined between the nodes,
        /// the returned path will follow that curve; otherwise, a straight line is used.
        /// </summary>
        /// <param name="fromPosition">The starting world position for the path calculation.</param>
        /// <param name="resolution">
        /// The number of segments used to approximate the Bézier curve. 
        /// Ignored for straight-line paths. Higher values result in smoother curves.
        /// </param>
        /// <returns>
        /// A <see cref="List{Vector3}"/> containing waypoints from the current position to the next node.
        /// </returns>
        public List<Vector3> GetPathToNextNode(Vector3 fromPosition, int resolution = 20)
        {
            List<Vector3> pathPoints = new List<Vector3>();

            Node nextNode = GetNextNodeByID(direction, endBehavior);
            if (nextNode == null) return pathPoints;

            bool useBezier = (currentNode?.SplineType == SplineType.Bezier || nextNode.SplineType == SplineType.Bezier);

            if (useBezier)
            {
                Vector3 p0, p1, p2, p3;

                if (direction == NodeTraversalDirection.Forward)
                {
                    p0 = fromPosition;
                    p1 = currentNode.handleOut != null ? currentNode.handleOut.position : p0;
                    p2 = nextNode.handleIn != null ? nextNode.handleIn.position : nextNode.transform.position;
                    p3 = nextNode.transform.position;
                }
                else
                {
                    p0 = fromPosition;
                    p1 = currentNode.handleIn != null ? currentNode.handleIn.position : p0;
                    p2 = nextNode.handleOut != null ? nextNode.handleOut.position : nextNode.transform.position;
                    p3 = nextNode.transform.position;
                }

                for (int i = 0; i <= resolution; i++)
                {
                    float t = i / (float)resolution;
                    pathPoints.Add(CalculateBezierPoint(t, p0, p1, p2, p3));
                }
            }
            else
            {
                pathPoints.Add(fromPosition);
                pathPoints.Add(nextNode.transform.position);
            }

            return pathPoints;
        }


        /// <summary>
        /// Returns the interpolated position along the path to the next node,
        /// based on a normalized time parameter t (0 to 1).
        /// </summary>
        /// <param name="t">Normalized time (0 to 1) along the path.</param>
        /// <param name="resolution">Number of segments for curve approximation.</param>
        /// <param name="useCurrentNodeAsStart">
        /// If true, the path starts from the current node's position instead of the transform's position.
        /// </param>
        /// <returns>Interpolated world position on the path.</returns>
        public Vector3? GetInterpolatedPosition(float t, int resolution = 20, bool useCurrentNodeAsStart = true)
        {
            Vector3 fromPosition = useCurrentNodeAsStart && currentNode != null
                ? currentNode.transform.position
                : transform.position;

            List<Vector3> path = GetPathToNextNode(fromPosition, resolution);
            if (path.Count < 2) return null;

            float totalLength = 0f;
            List<float> segmentLengths = new List<float>();

            // Calculate segment lengths and total path length
            for (int i = 0; i < path.Count - 1; i++)
            {
                float segLen = Vector3.Distance(path[i], path[i + 1]);
                segmentLengths.Add(segLen);
                totalLength += segLen;
            }

            float targetDistance = t * totalLength;
            float accumulated = 0f;

            for (int i = 0; i < segmentLengths.Count; i++)
            {
                if (accumulated + segmentLengths[i] >= targetDistance)
                {
                    float segmentT = (targetDistance - accumulated) / segmentLengths[i];
                    return Vector3.Lerp(path[i], path[i + 1], segmentT);
                }

                accumulated += segmentLengths[i];
            }

            return path[^1]; // fallback to last point
        }



        /// <summary>
        /// Moves to the next node in the graph based on the default direction and end behavior.
        /// If the end of the graph is reached, traversal is stopped.
        /// </summary>

        [Command]
        public void MoveToNextNode()
        {
            if (!IsGraphValid()) return;

            Node nextNode = GetNextNodeByID(direction, endBehavior);

            if (nextNode == null)
            {
                OnTraversalStopped?.Invoke(currentNode);
                return;
            }

            currentNode = nextNode;
            OnNodeChanged?.Invoke(currentNode);
            DebugMessage($"Moved to node: {currentNode.name}");
        }


        /// <summary>
        /// Moves to the next node in the graph using the specified traversal direction and end behavior.
        /// If the end of the graph is reached, traversal is stopped.
        /// </summary>
        /// <param name="direction">The direction of traversal (e.g., forward or backward).</param>
        /// <param name="endBehavior">Defines what happens when the end of the graph is reached.</param>
        public void MoveToNextNode(NodeTraversalDirection direction, TraversalEndBehavior endBehavior)
        {
            if (!IsGraphValid()) return;

            Node nextNode = GetNextNodeByID(direction, endBehavior);

            if (nextNode == null)
            {
                OnTraversalStopped?.Invoke(currentNode);
                return;
            }

            currentNode = nextNode;
            OnNodeChanged?.Invoke(currentNode);
            DebugMessage($"Moved to node: {currentNode.name}");
        }


        /// <summary>
        /// Moves directly to the node at the specified index in the graph.
        /// Logs a warning if the index is out of bounds.
        /// </summary>
        /// <param name="index">The index of the target node in the graph.</param>
        [Command]
        public void MoveToNodeByIndex(int index)
        {
            if (!IsGraphValid()) return;
            if (index < 0 || index >= nodeGraph.Nodes.Count)
            {
                Debug.LogWarning($"Index {index} is out of range.");
                return;
            }

            currentNode = nodeGraph.Nodes[index];
            OnNodeChanged?.Invoke(currentNode);
            DebugMessage($"Moved to node by index: {currentNode.name}");
        }


        /// <summary>
        /// Moves directly to the node at the specified ID in the graph.
        /// Logs a warning if the ID does not exist.
        /// </summary>
        /// <param name="ID">The ID of the target node in the graph.</param>
        [Command]
        public void MoveToNodeByID(int ID)
        {
            if (!IsGraphValid()) return;

            Node node = nodeGraph.GetNodeByID(ID);

            if (node == null)
            {
                Debug.LogWarning($"The node with the ID '{ID}' does not exist.");
                return;
            }

            currentNode = node;
            OnNodeChanged?.Invoke(currentNode);
            DebugMessage($"Moved to node by ID: {currentNode.name}");
        }

        public Node GetCurrentNode()
        {
            return currentNode;
        }

        public void SetDirection(NodeTraversalDirection newDirection)
        {
            direction = newDirection;
        }

        public void SetEndBehaviour(TraversalEndBehavior newEndBehaviour)
        {
            endBehavior = newEndBehaviour;
        }


        #endregion





        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 point = uuu * p0;
            point += 3 * uu * t * p1;
            point += 3 * u * tt * p2;
            point += ttt * p3;

            return point;
        }

        private Node GetNextNodeByID(NodeTraversalDirection direction, TraversalEndBehavior endBehavior)
        {
            if (currentNode == null || currentNode.ID < 0 || currentNode.ID >= nodeGraph.Nodes.Count)
            {
                Debug.LogWarning("Current node is not in the graph.");
                return null;
            }

            int currentID = currentNode.ID;
            int nextID = direction == NodeTraversalDirection.Forward ? currentID + 1 : currentID - 1;

            Node nextNode = nodeGraph.GetNodeByID(nextID);

            if (nextNode != null)
            {
                return nextNode;
            }

            switch (endBehavior)
            {
                case TraversalEndBehavior.LoopToInitial:
                    return initialNode != null && nodeGraph.Nodes.Contains(initialNode) ? nodeGraph.GetNodeByID(initialNode.ID) : nodeGraph.GetNodeByID(0);

                case TraversalEndBehavior.LoopToEdge:
                    return direction == NodeTraversalDirection.Forward ? nodeGraph.GetNodeByID(0) : nodeGraph.GetNodeByID(nodeGraph.Nodes.Count - 1);

                case TraversalEndBehavior.Stop:
                    Debug.Log("Traversal has reached the end and is stopping.");
                    return null;

                default:
                    return null;
            }
        }

        private bool IsGraphValid()
        {
            return nodeGraph != null && nodeGraph.Nodes != null && nodeGraph.Nodes.Count > 0;
        }

        private void DebugMessage(object message)
        {
            if (!showDebugs) return;

            Debug.Log(message);
        }





        public enum TraversalEndBehavior
        {
            LoopToInitial,   // Go back to the initial established node.
            LoopToEdge,      // Go back to the start or end of the node.
            Stop             // Do nothig;
        }

        public enum NodeTraversalDirection
        {
            Forward,  // From lowest to highest ID (ex: 0 ? 1 ? 2 ? 3)
            Backward  // De highest to lowest ID (ex: 3 ? 2 ? 1 ? 0)
        }


        #region TestMethods


        [Command]
        public void TestPathToNextNode(int resolution = 20)
        {
            Vector3 fromPosition = transform.position;
            List<Vector3> pathPoints = GetPathToNextNode(fromPosition, resolution);

            if (pathPoints.Count < 2)
            {
                Debug.LogWarning("Not enough points to draw the path.");
                return;
            }

#if UNITY_EDITOR
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Debug.DrawLine(pathPoints[i], pathPoints[i + 1], Color.cyan, 10f);
            }

            Debug.Log($"[TEST] Displaying path from {pathPoints[0]} to {pathPoints[pathPoints.Count - 1]} with {pathPoints.Count} points.");
#else
    Debug.LogWarning("This method should only be used in the Editor.");
#endif
        }



        #endregion
    }
}



