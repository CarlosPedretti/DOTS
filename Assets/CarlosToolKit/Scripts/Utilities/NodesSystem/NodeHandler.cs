using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Utilities
{
    public class NodeHandler : MonoBehaviour, INodeElement
    {
        [HideInInspector][SerializeField] private NodeGraph nodeGraph;
        [HideInInspector][SerializeField] private Node referencedNode;
        [HideInInspector][SerializeField] private NodeHandlerType type;

        private bool isSelected;

        [HideInInspector][SerializeField] BoxCollider boxCollider;

        public void Init(NodeGraph nodeGraph, Node referencedNode, NodeHandlerType type)
        {
            this.nodeGraph = nodeGraph;
            this.referencedNode = referencedNode;
            this.type = type;

            boxCollider = this.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;

            this.nodeGraph.OnConfigurationChanged.AddListener(OnConfigurationChangedEvent);

            UpdateHandles();
            UpdateColliderEvent();
        }

        void OnValidate()
        {
            if (nodeGraph != null) nodeGraph.OnConfigurationChanged.AddListener(OnConfigurationChangedEvent);
        }

        private void OnDestroy()
        {
            nodeGraph.OnConfigurationChanged.RemoveListener(OnConfigurationChangedEvent);
        }

        #region GUI

        private void OnDrawGizmos()
        {
            if (nodeGraph == null || !nodeGraph.Config.ShowVisuals) return;

            UpdateHandles();
        }

        void OnConfigurationChangedEvent()
        {
            UpdateColliderEvent();
        }


        private void UpdateHandles()
        {
            if (referencedNode.SplineType == SplineType.Linear) return;

            Color color;

            isSelected = ReferenceEquals(nodeGraph.SelectedNodeElement, this);

            if (isSelected)
            {
                color = nodeGraph.visualSettings.selectedNodeColor;
            }
            else
            {
                color = type == NodeHandlerType.In ? nodeGraph.visualSettings.handleInColor : nodeGraph.visualSettings.handleOutColor;
            }


            Gizmos.color = color;
            Gizmos.DrawCube(transform.position, GetCubeSizeFromSphereRadius());
            Gizmos.DrawLine(referencedNode.transform.position, transform.position);
        }

        private void UpdateColliderEvent()
        {
            boxCollider.size = GetCubeSizeFromSphereRadius();
        }

        private Vector3 GetCubeSizeFromSphereRadius()
        {
            float cubeSide = nodeGraph.visualSettings.nodeRadius * 2f * 0.2f; // diameter reduced by a 80%
            return new Vector3(cubeSide, cubeSide, cubeSide);
        }


        #endregion
    }

    public enum NodeHandlerType 
    {
        In,
        Out,
    }

}

