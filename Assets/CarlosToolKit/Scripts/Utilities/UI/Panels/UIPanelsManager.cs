using QFSW.QC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.UI
{
    public class UIPanelsManager : Singleton<UIPanelsManager>
    {
        [SerializeField] bool activateInitialPanelOnStart = false;
        [SerializeField] string initialPanelKey;

        [SerializeField] private List<UIPanel> rootPanels = new List<UIPanel>();

        private Dictionary<string, UIPanel> panelDictionary = new Dictionary<string, UIPanel>();
        private Stack<UIPanel> panelHistory = new Stack<UIPanel>();
        [SerializeField] private UIPanel currentPanel;

        public UIPanel CurrentPanel { get { return currentPanel; } }

        //Events
        public event Action<UIPanel> OnPanelShown;
        public event Action<UIPanel> OnPanelHidden;

        //<New selected panel, previous panel>
        public event Action<UIPanel, UIPanel> OnPanelChanged;

        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        protected override void Start()
        {
            base.Start();

            if (activateInitialPanelOnStart) SelectPanel(initialPanelKey);
        }

        [Command]
        public void SelectPanel(string key, bool registerChangeInHistory = true)
        {
            if (!panelDictionary.TryGetValue(key, out UIPanel panel)) return;

            if (currentPanel == panel) return;

            ChangePanel(panel, registerChangeInHistory);
        }

        [Command]
        public void SelectSubPanel(string parentKey, string subPanelKey, bool registerChangeInHistory = true)
        {
            if (!panelDictionary.TryGetValue(parentKey, out UIPanel parentPanel)) return;

            UIPanel subPanel = parentPanel.GetSubPanel(subPanelKey);
            if (subPanel == null) return;

            if (currentPanel == subPanel) return;

            var previous = currentPanel;

            if (!parentPanel.IsActive)
                parentPanel.Show();

            parentPanel.NotifySubPanelChange(subPanel, previous);

            ChangePanel(subPanel, registerChangeInHistory);
        }

        [Command]
        public void SelectPreviousPanel()
        {
            if (panelHistory.Count > 0)
            {
                var previousPanel = panelHistory.Pop();

                if (previousPanel.IsSubPanel)
                {
                    ChangePanel(previousPanel.ParentPanel, registerChangeInHistory: false);
                    ChangePanel(previousPanel, registerChangeInHistory: false);
                }
                else
                {
                    ChangePanel(previousPanel, registerChangeInHistory: false);
                }

                Debug.Log($"[UIPanelsManager] Went back to {previousPanel.PanelKey}. Remaining history: {panelHistory.Count}");
            }
            else
            {
                Debug.LogWarning("[UIPanelsManager] No previous panel in history.");
            }
        }

        private void ChangePanel(UIPanel newPanel, bool registerChangeInHistory = true)
        {
            if (newPanel == null) return;
            if (newPanel == currentPanel) return;

            var previous = currentPanel;

            bool isSibling = previous != null &&
                             previous.IsSubPanel &&
                             newPanel.IsSubPanel &&
                             previous.ParentPanel == newPanel.ParentPanel;

            bool isChild = previous != null &&
                           !previous.IsSubPanel &&
                           newPanel.IsSubPanel &&
                           newPanel.ParentPanel == previous;

            if (previous != null && registerChangeInHistory)
                panelHistory.Push(previous);

            if (isChild)
            {
                // Case 1: New panel is a child – keep the parent, only show the child
                currentPanel = newPanel;
                currentPanel.Show();
                OnPanelShown?.Invoke(currentPanel);
            }
            else if (isSibling)
            {
                // Case 2: Sibling panels – hide the previous one, show the new one
                previous.Hide();
                OnPanelHidden?.Invoke(previous);

                currentPanel = newPanel;
                currentPanel.Show();
                OnPanelShown?.Invoke(currentPanel);
            }
            else
            {
                // Case 3: Different root or no relation – hide the entire parent chain
                HideWithParents(previous);

                currentPanel = newPanel;
                currentPanel.Show();
                OnPanelShown?.Invoke(currentPanel);
            }

            OnPanelChanged?.Invoke(currentPanel, previous);
        }

        /// <summary>
        /// Hides the panel and all of its parent panels recursively
        /// </summary>
        private void HideWithParents(UIPanel panel)
        {
            while (panel != null)
            {
                panel.Hide();
                OnPanelHidden?.Invoke(panel);
                panel = panel.ParentPanel;
            }
        }

        private void Initialize()
        {
            DictionaryInitialization();

            foreach (var panel in rootPanels)
            {
                panel.InitializePanel();
            }
        }

        private void DictionaryInitialization()
        {
            foreach (var panel in rootPanels)
            {
                if (panel == null) continue;

                if (!panelDictionary.ContainsKey(panel.PanelKey))
                {
                    panelDictionary.Add(panel.PanelKey, panel);
                }
                else
                {
                    Debug.LogWarning($"[UIPanelsManager] The panel with the key word {panel.PanelKey} already exists.");
                }
            }
        }

        [Command]
        public void TestPanelHistory()
        {
            if (panelHistory == null || panelHistory.Count == 0)
            {
                Debug.Log("[UIPanelsManager] Panel history is empty.");
                return;
            }

            foreach (var panel in panelHistory)
            {
                Debug.Log($"[UIPanelsManager] Panel in history: {panel.PanelKey}");
            }
        }

        [Command]
        private void TestCheckPanels()
        {
            foreach (var panel in panelDictionary)
            {
                Debug.Log($"[UIPanelsManager] Panels: {panel.Value.PanelKey}");
            }
        }
    }

}
