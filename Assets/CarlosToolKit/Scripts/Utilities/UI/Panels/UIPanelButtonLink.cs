using UnityEngine;
using UnityEngine.UI;

namespace Utilities.UI
{
    [RequireComponent(typeof(Button))]
    public class UIPanelButtonLink : MonoBehaviour
    {
        [Tooltip("Should this button visually highlight when its panel is active?")]
        [SerializeField] private bool highlightWhenPanelIsActive;

        [Tooltip("The panel to control when this button is clicked.")]
        [SerializeField] private UIPanel panel;

        [Tooltip("Action performed when button is clicked.")]
        [SerializeField] private ButtonAction action = ButtonAction.Open;

        private Color initialButtonImageColor;

        public enum ButtonAction
        {
            Open,
            SelectPrevious,
        }

        private Button button;

        private void OnEnable()
        {
            if (panel == null || UIPanelsManager.Instance == null) return;

            UIPanelsManager.Instance.OnPanelChanged += ManageButtonHighlight;

            if (panel == UIPanelsManager.Instance.CurrentPanel)
            {
                ManageButtonHighlight(panel, default);
            }
        }


        private void OnDisable()
        {
            if (panel == null) return;

            panel.OnSubPanelChanged -= ManageButtonHighlight;
        }

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleButtonClick);
            initialButtonImageColor = button.image.color;
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleButtonClick);
        }

        private void HandleButtonClick()
        {
            switch (action)
            {
                case ButtonAction.Open:

                    if (panel == null)
                    {
                        Debug.LogWarning($"{nameof(UIPanelButtonLink)} on {gameObject.name} has no panel assigned.");
                        return;
                    }

                    if (panel.IsSubPanel)
                    {
                        UIPanelsManager.Instance.SelectSubPanel(panel.ParentPanel.PanelKey, panel.PanelKey);
                    }
                    else
                    {
                        UIPanelsManager.Instance.SelectPanel(panel.PanelKey);
                    }

                    break;

                case ButtonAction.SelectPrevious:

                    UIPanelsManager.Instance.SelectPreviousPanel();
                    break;

            }
        }

        private void ManageButtonHighlight(UIPanel newSelectedSubPanel, UIPanel previousSubPanel)
        {
            if (!highlightWhenPanelIsActive) return;

            if (panel == newSelectedSubPanel)
            {
                button.targetGraphic.color = button.colors.highlightedColor;
            }
            else
            {
                button.targetGraphic.color = initialButtonImageColor;
            }
        }
    }
}
