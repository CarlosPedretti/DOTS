using UnityEngine;
using VInspector;

namespace Utilities.UI
{
    [CreateAssetMenu(fileName = "New UIPanel Input Config", menuName = "Utilities/UIPanel Cursor Config", order = 1)]
    public class UIPanelCursorConfig : ScriptableObject
    {
        [Header("Cursor Control")]
        public bool cursorVisible;
        public CursorLockMode cursorLockMode;

    }
}