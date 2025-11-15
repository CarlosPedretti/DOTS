using UnityEngine;
using Utilities.Input;

namespace Utilities.UI
{
    [CreateAssetMenu(fileName = "New UIPanel Input Config", menuName = "Utilities/UIPanel Input Config", order = 0)]
    public class UIPanelInputConfig : ScriptableObject
    {
        [Header("Input Control")]
        public InputActionModificationConfig modifyInputActionOnShow;
        public InputActionModificationConfig modifyInputActionOnHide;
    }
}
