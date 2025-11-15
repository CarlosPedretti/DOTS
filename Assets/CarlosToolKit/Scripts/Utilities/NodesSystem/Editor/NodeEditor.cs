using UnityEditor;

namespace Utilities
{
    [CustomEditor(typeof(Node))]
    public class NodeEditor : Editor
    {
        Node pathNode;

        private void OnEnable()
        {
            pathNode = (Node)target;
        }

        private void OnSceneGUI()
        {

        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

    }

}
