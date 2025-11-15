#if UNITY_EDITOR 
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;
using Utilities.Input;

namespace Utilities.UI
{
    [RequireComponent(typeof(UIAnimator))]
    public class UIPanel : MonoBehaviour
    {
        [SerializeField] private string panelKey;

        public string PanelKey => panelKey;
        public UIPanel ParentPanel { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsSubPanel => ParentPanel != null;
        public bool HasSubPanels => subPanels != null && subPanels.Count > 0;


        public UnityEvent OnShowEvent;
        public UnityEvent OnHideEvent;

        //<New selected panel, previous panel>
        public event Action<UIPanel, UIPanel> OnSubPanelChanged;

        public void Show()
        {
            //Do not modify Show()
            IsActive = true;

            SelectInitialSubPanel();

            SelectFirstSelectable();

            if (inputConfig != null)
            {
                ManageLocalInputCurrentActionMap(inputConfig.modifyInputActionOnShow);
                ManageLocalInputActionsStates(inputConfig.modifyInputActionOnShow);
            }

            OnShow();

            OnShowEvent?.Invoke();
        }

        public void Hide()
        {
            //Do not modify Hide()

            IsActive = false;

            if (inputConfig != null)
            {
                ManageLocalInputCurrentActionMap(inputConfig.modifyInputActionOnHide);
                ManageLocalInputActionsStates(inputConfig.modifyInputActionOnHide);
            }

            OnHide();

            OnHideEvent?.Invoke();
        }

        protected virtual void OnShow()
        {
            //Modify here
            ManageCursorState(cursorConfigOnShow);

            HandleShowAnimation();
        }

        protected virtual void OnHide()
        {
            //Modify here

            ManageCursorState(cursorConfigOnHide);

            HandleHideAnimation();
        }

        public UIPanel GetSubPanel(string key)
        {
            if (subPanelDictionary.TryGetValue(key, out UIPanel panel))
            {
                currentSubPanel = panel;
                return panel;
            }

            Debug.LogWarning($"[UIPanel] SubPanel with key {key} not found in {PanelKey}");
            return null;
        }

        public void NotifySubPanelChange(UIPanel newSubPanel, UIPanel oldSubPanel)
        {
            OnSubPanelChanged?.Invoke(newSubPanel, oldSubPanel);
        }


        #region SubPanel Management

        [SerializeField] bool selectInitialSubPanel = false;
        [SerializeField] string initialSubPanelName;

        [SerializeField] private List<UIPanel> subPanels = new List<UIPanel>();
        private Dictionary<string, UIPanel> subPanelDictionary = new Dictionary<string, UIPanel>();

        private UIPanel currentSubPanel;


        public void InitializePanel()
        {
            DictionaryInitialization();

            animator = GetComponent<UIAnimator>();

            gameObject.SetActive(false);
        }

        private void DictionaryInitialization()
        {
            foreach (var panel in subPanels)
            {
                if (panel == null) continue;

                if (!subPanelDictionary.ContainsKey(panel.PanelKey))
                {
                    subPanelDictionary.Add(panel.PanelKey, panel);
                    panel.ParentPanel = this;
                    gameObject.SetActive(false);

                }
                else
                {
                    Debug.LogWarning($"[UIPanel] The SubPanel with the key word {panel.PanelKey} already exists.");
                }
            }
        }

        private void SelectInitialSubPanel()
        {
            if (!selectInitialSubPanel || !HasSubPanels) return;

            if (!string.IsNullOrEmpty(initialSubPanelName))
            {
                UIPanelsManager.Instance.SelectSubPanel(PanelKey, initialSubPanelName, registerChangeInHistory: false);
            }
            else
            {
                UIPanel firstPanel = subPanels[0];
                if (firstPanel != null)
                {
                    UIPanelsManager.Instance.SelectSubPanel(PanelKey, firstPanel.PanelKey, registerChangeInHistory: false);
                }
            }
        }

        #endregion

        #region Animation Management
        [SerializeField] UIAnimationRoutine showAnimationRoutine;
        [SerializeField] UIAnimationRoutine hideAnimationRoutine;

        private UIAnimator animator;

        private void HandleShowAnimation()
        {
            if (animator == null || showAnimationRoutine == null)
            {
                gameObject.SetActive(true);
                return;
            }

            gameObject.SetActive(true);
            animator.KillActiveTween(resetToInitial: true);
            animator.PlayRoutine(showAnimationRoutine);

            animator.ActiveTween?.OnComplete(() =>
            {

            });

            //Debug.Log($"Show() {panelKey}");
        }

        private void HandleHideAnimation()
        {
            if (animator == null || hideAnimationRoutine == null)
            {
                gameObject.SetActive(false);
                return;
            }

            animator.KillActiveTween(resetToInitial: true);
            animator.PlayRoutine(hideAnimationRoutine);

            animator.ActiveTween?.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

            //Debug.Log($"Hide() {panelKey}");
        }

        #endregion

        #region Input Management

        [SerializeField] private UIPanelInputConfig inputConfig;

        private void ManageLocalInputActionsStates(InputActionModificationConfig modifyInputActionState)
        {
            if (inputConfig == null) return;

            if (!modifyInputActionState.ModifyCurrentMapActions) return;

            if (LocalInputManager.Instance.RegisteredInputs.Count == 0) return;

            LocalInputManager.Instance.ForEachInput(localInput =>
            {
                localInput.ChangeActionsStatesExceptFor(modifyInputActionState.ActionState, modifyInputActionState.IgnoredActions);
            });
        }

        private void ManageLocalInputCurrentActionMap(InputActionModificationConfig modifyInputActionState)
        {
            if (!modifyInputActionState.ModifyCurrentActionMap) return;

            if (LocalInputManager.Instance.RegisteredInputs.Count == 0) return;

            LocalInputManager.Instance.ForEachInput(localInput =>
            {
                switch (modifyInputActionState.ActionMapChangeMode)
                {
                    case ActionMapChangeModeEnum.SwitchActionMap:

                        localInput.SwitchCurrentActionMap(modifyInputActionState.ActionMapToSwitch);

                        break;

                    case ActionMapChangeModeEnum.EnableOrDisableActionMaps:

                        if (modifyInputActionState.ActionsMapsToEnableOrDisable.Length == 0) return;

                        foreach (var action in modifyInputActionState.ActionsMapsToEnableOrDisable)
                        {
                            localInput.SetActionMapState(action.ActionMapName, action.ActionMapState);
                        }

                        break;
                }
            });
        }

        #endregion

        #region Cursor Management

        [SerializeField] private UIPanelCursorConfig cursorConfigOnShow;
        [SerializeField] private UIPanelCursorConfig cursorConfigOnHide;

        private void ManageCursorState(UIPanelCursorConfig cursorConfig)
        {
            if (cursorConfig == null) return;

            Cursor.visible = cursorConfig.cursorVisible;
            Cursor.lockState = cursorConfig.cursorLockMode;
        }

        #endregion

        #region Button Management

        [SerializeField] private Selectable defaultSelectable;

        void SelectFirstSelectable()
        {
            Selectable target = defaultSelectable;

            if (target == null)
            {
                target = GetComponentInChildren<Selectable>();
            }

            if (target == null)
            {
                //Debug.LogWarning($"[UIPanel] No selectable found in panel {PanelKey}");
                return;
            }

            LocalInputManager.Instance.ForEachInput(input =>
            {
                var eventSystem = input.EventSystem;
                if (eventSystem != null)
                {
                    //Debug.Log($"[UIPanel] Trying to select '{target.gameObject.name}' for Player {input.PlayerInput?.playerIndex}");

                    eventSystem.SetSelectedGameObject(null);
                    eventSystem.SetSelectedGameObject(target.gameObject);

                    //Debug.Log($"[UIPanel] Selected '{eventSystem.currentSelectedGameObject?.name}' for Player {input.PlayerInput?.playerIndex}");
                }
                else
                {
                    Debug.LogWarning($"[UIPanel] No EventSystem found for Player {input.PlayerInput?.playerIndex}");
                }
            });
        }


        #endregion

    }


#if UNITY_EDITOR

    [CustomEditor(typeof(UIPanel))]
    public class NewUIPanelEditor : Editor
    {
        //Main Settings
        SerializedProperty panelKeyProp;
        SerializedProperty defaultSelectableProp;

        //Sub Panels Section
        SerializedProperty selectInitialSubPanelProp;
        SerializedProperty initialSubPanelNameProp;
        SerializedProperty subPanelsProp;

        //Input Section
        SerializedProperty inputConfigProp;

        //Animation Section
        SerializedProperty showAnimationRoutineProp;
        SerializedProperty hideAnimationRoutineProp;

        //Cursor Section
        SerializedProperty cursorConfigOnShowProp;
        SerializedProperty cursorConfigOnHideProp;

        //Events Section
        SerializedProperty OnShowEventProp;
        SerializedProperty OnHideEventProp;


        // Foldout states
        bool showSubPanels;
        bool showAnimationRoutine;
        bool showEvents;
        bool showCursorControl;



        void OnEnable()
        {
            //Main Settings
            panelKeyProp = Find("panelKey");
            defaultSelectableProp = Find("defaultSelectable");

            //Sub Panels Section
            selectInitialSubPanelProp = Find("selectInitialSubPanel");
            initialSubPanelNameProp = Find("initialSubPanelName");
            subPanelsProp = Find("subPanels");

            //Input Section
            inputConfigProp = Find("inputConfig");

            //Animation Section
            showAnimationRoutineProp = Find("showAnimationRoutine");
            hideAnimationRoutineProp = Find("hideAnimationRoutine");

            //Cursor Section
            cursorConfigOnShowProp = Find("cursorConfigOnShow");
            cursorConfigOnHideProp = Find("cursorConfigOnHide");

            //Events Section
            OnShowEventProp = Find("OnShowEvent");
            OnHideEventProp = Find("OnHideEvent");
        }

        SerializedProperty Find(string name) => serializedObject.FindProperty(name);

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawMainSettings();
            DrawSubPanelSection();
            DrawInputSection();
            DrawAnimationSection();
            DrawCursorSection();
            DrawEventsSection();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawMainSettings()
        {
            EditorGUILayout.PropertyField(panelKeyProp);
            EditorGUILayout.PropertyField(defaultSelectableProp);
            EditorGUILayout.Space();
        }

        void DrawSubPanelSection()
        {
            showSubPanels = EditorGUILayout.Foldout(showSubPanels, "Sub Panel Configuration", true);
            if (showSubPanels)
            {
                EditorGUILayout.PropertyField(selectInitialSubPanelProp, true);
                EditorGUILayout.PropertyField(initialSubPanelNameProp, true);
                EditorGUILayout.PropertyField(subPanelsProp, true);
            }

            EditorGUILayout.Space();
        }

        void DrawInputSection()
        {
            inputConfigProp.isExpanded = EditorGUILayout.Foldout(inputConfigProp.isExpanded, "Input Configuration", true);
            if (inputConfigProp.isExpanded)
            {
                EditorGUILayout.PropertyField(inputConfigProp, true);
            }
            EditorGUILayout.Space();
        }

        void DrawAnimationSection()
        {
            showAnimationRoutine = EditorGUILayout.Foldout(showAnimationRoutine, "Animation Configs", true);

            if (showAnimationRoutine)
            {
                EditorGUILayout.PropertyField(showAnimationRoutineProp, true);
                EditorGUILayout.PropertyField(hideAnimationRoutineProp, true);
            }
        }

        void DrawCursorSection()
        {
            showCursorControl = EditorGUILayout.Foldout(showCursorControl, "Cursor Configuration", true);

            if (showCursorControl)
            {
                EditorGUILayout.PropertyField(cursorConfigOnShowProp, true);
                EditorGUILayout.PropertyField(cursorConfigOnHideProp, true);
            }
        }

        void DrawEventsSection()
        {
            showEvents = EditorGUILayout.Foldout(showEvents, "UIPanel Events", true);
            if (showEvents)
            {
                EditorGUILayout.PropertyField(OnShowEventProp, true);
                EditorGUILayout.PropertyField(OnHideEventProp, true);
            }
        }
    }


#endif

}



