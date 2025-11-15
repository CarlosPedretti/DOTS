using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Utilities
{
    [Icon("Assets/CarlosToolKit/Scripts/Utilities/NodesSystem/Editor/Resources/Icons/NodeToolIcon.png")]
    [EditorTool("Node Tool")]
    public class NodeTool : EditorTool
    {
        [SerializeField] private NodeGraph nodeGraph;

        [SerializeField] private INodeElement clickedNodeElement;

        [SerializeField] private Node clickedNode
        {
            get
            {
                if (clickedNodeElement is Node)
                {
                    return clickedNodeElement as Node;
                }

                return null;
            }
        }

        private bool isConnectMode = false;


        public override void OnActivated()
        {

        }

        public override void OnWillBeDeactivated()
        {
            if (!IsAvailable()) return;

            nodeGraph.SetSelectedNodeElement(null);

            if(nodeGraph.Config.AutoBake) nodeGraph.BakeGraph();
        }

        public override bool IsAvailable()
        {
            return Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<NodeGraph>() != null;
        }


        public override void OnToolGUI(EditorWindow window)
        {
            SceneView sceneView = window as SceneView;

            if (Selection.activeGameObject == null) return;

            nodeGraph = Selection.activeGameObject.GetComponent<NodeGraph>();
            if (nodeGraph == null) return;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Event e = Event.current;

            HandleKeyboardEvents(e);
            HandleMouseEvents(e);

        }

        private void HandleMouseEvents(Event e)
        {
            if (e.alt) return;

            switch (e.type)
            {
                case EventType.MouseDown:
                    HandleMouseDown(e);
                    break;

                case EventType.MouseDrag:
                    HandleMouseDrag(e);
                    break;

                case EventType.MouseUp:
                    HandleMouseUp(e);
                    break;
            }
        }

        private void HandleKeyboardEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.KeyDown:

                    HandleKeyboardDown(e);

                    break;

                case EventType.KeyUp:

                    HandleKeyboardUp(e);

                    break;

            }
        }



        #region MouseEvents

        private void HandleMouseDown(Event e)
        {
            if (e.button != 0) return; // Only left button

            NodeClick(e);
        }

        private void HandleMouseUp(Event e)
        {
            if (e.button != 0) return;

        }

        private void HandleMouseDrag(Event e)
        {
            if (e.button != 0 || clickedNodeElement == null) return;

            DragNode(clickedNodeElement, e.mousePosition);
            e.Use();
        }


        #endregion

        #region Keyboard Events

        private void HandleKeyboardDown(Event e)
        {
            if (e.keyCode == nodeGraph.KeyBindings.DeleteKey)
            {
                DeleteNode(e);
            }
            else if (e.keyCode == nodeGraph.KeyBindings.EscKey)
            {
                Escape(e);
            }
            else if (e.keyCode == nodeGraph.KeyBindings.MoveKey)
            {
                MoveNode(e);
            }
            else if (e.keyCode == nodeGraph.KeyBindings.ConnectKey)
            {
                isConnectMode = true;
                e.Use();
            }
        }

        private void HandleKeyboardUp(Event e)
        {
            if (e.keyCode == nodeGraph.KeyBindings.ConnectKey)
            {
                isConnectMode = false;
                e.Use();
            }
        }

        #endregion


        #region Input Handlers

        void NodeClick(Event e)
        {
            INodeElement nodeElementUnderMouse = RaycastComponentUnderMouse<INodeElement>(e.mousePosition);

            clickedNodeElement = nodeElementUnderMouse;

            if (nodeElementUnderMouse == null)
            {
                CreateNode(e);
            }
            else
            {
                SelectNode(e);
            }

            void CreateNode(Event e)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    CreateAndConnectNode(hit.point);
                }
                else
                {
                    Plane plane = new Plane(Vector3.up, Vector3.zero);
                    if (plane.Raycast(ray, out float enter))
                    {
                        Vector3 point = ray.GetPoint(enter);
                        CreateAndConnectNode(point);
                    }
                }

                e.Use();
            }

            void SelectNode(Event e)
            {
                if (clickedNodeElement is Node)
                {
                    if (clickedNode != nodeGraph.CurrentSelectedNode && isConnectMode)
                    {
                        TryCreateConnection(nodeGraph.CurrentSelectedNode, clickedNode);
                    }
                    else
                    {
                        nodeGraph.SetSelectedNodeElement(clickedNode);
                        NodeInspectorWindow.ShowWindow(nodeGraph);
                    }
                }
                else
                {
                    nodeGraph.SetSelectedNodeElement(clickedNodeElement);
                }

                e.Use();
            }
        }

        private void DeleteNode(Event e)
        {
            if (nodeGraph.CurrentSelectedNode == null)
                return;

            Undo.DestroyObjectImmediate(nodeGraph.CurrentSelectedNode.gameObject);
            nodeGraph.RemoveNode(nodeGraph.CurrentSelectedNode as Node);
            nodeGraph.SetSelectedNodeElement(null);
            MarkDirtyAndUseEvent(e);
        }

        private void Escape(Event e)
        {
            nodeGraph.SetSelectedNodeElement(null);
            MarkDirtyAndUseEvent(e);
            SceneView.RepaintAll();
        }

        private void MoveNode(Event e)
        {
            if (clickedNodeElement == null)
                return;

            Transform nodeTransform = clickedNodeElement.transform;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                nodeTransform.position = hit.point;
            }
            else
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(ray, out float enter))
                {
                    nodeTransform.position = ray.GetPoint(enter);
                }
            }
        }



        #endregion



        #region Auxiliar Methods

        private void TryCreateConnection(Node from, Node to)
        {
            Undo.RecordObject(nodeGraph, "Create Connection");

            if (nodeGraph.TryCreateConnection(from, to))
            {
                EditorUtility.SetDirty(nodeGraph);
            }
        }

        private void CreateAndConnectNode(Vector3 position)
        {
            var newNode = nodeGraph.CreateNode(position);
            nodeGraph.ConnectNode(newNode);

            EditorUtility.SetDirty(nodeGraph);
        }

        private void DragNode(INodeElement node, Vector2 mousePosition)
        {
            Transform nodeTransform = node.transform;
            float originalY = nodeTransform.position.y;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, originalY, 0));

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 newPosition = ray.GetPoint(enter);
                newPosition.y = originalY;

                Undo.RecordObject(nodeTransform, "Move Node");
                nodeTransform.position = newPosition;
                EditorUtility.SetDirty(nodeTransform);
            }
        }

        private void MarkDirtyAndUseEvent(Event e)
        {
            EditorUtility.SetDirty(nodeGraph);
            e.Use();
        }

        private T RaycastComponentUnderMouse<T>(Vector2 mousePos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.collider.GetComponent<T>();
            }

            return default;
        }

        #endregion


    }
}

