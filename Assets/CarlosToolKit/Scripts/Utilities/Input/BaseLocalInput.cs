using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Utilities.UI;

namespace Utilities.Input
{
    /// <summary>
    /// Base class for handling local input using Unity's Input System.
    /// Provides functionality to manage control schemes, action maps, and action states.
    /// Exposes events for reacting to changes in input configuration and triggered actions.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class BaseLocalInput : MonoBehaviour
    {
        [Header("Configurations")]
        [Space(2)]
        [SerializeField] protected bool showDebugs = true;
        [SerializeField] InputActionModificationConfig initialConfig;

        [Space(5)]

        [Header("Current Control Scheme and Active Actions")]
        [Space(2)]
        [SerializeField] protected string currentControlScheme;
        public string CurrentControlScheme { get { return currentControlScheme; } }
        [Space]
        [SerializeField] private List<string> currentlyEnabledActionMaps = new List<string>();
        [SerializeField] private List<string> currentlyEnabledActions = new List<string>();
        public IReadOnlyList<string> CurrentlyEnabledActionMaps => currentlyEnabledActionMaps;
        public IReadOnlyList<string> CurrentlyEnabledActions => currentlyEnabledActions;

        public int PlayerIndex => PlayerInput.playerIndex;

        private PlayerInput playerInput;

        public PlayerInput PlayerInput
        {
            get
            {
                if (playerInput == null)
                    playerInput = GetComponentInChildren<PlayerInput>(true);

                return playerInput;
            }
        }

        private EventSystem eventSystem;

        public EventSystem EventSystem
        {
            get
            {
                if (eventSystem == null)
                {

                    eventSystem = GetComponentInChildren<EventSystem>(true);

                    if (eventSystem == null)
                    {
                        GameObject go = new GameObject("MultiplayerEventSystem", typeof(MultiplayerEventSystem), typeof(InputSystemUIInputModule));
                        go.transform.SetParent(transform, false);
                        eventSystem = go.GetComponent<EventSystem>();
                    }
                }

                return eventSystem;
            }
        }

        #region Events

        public event Action<PlayerInput> OnControlSchemeChanged;
        public event Action<string> OnActionMapChanged;
        public event Action OnActionsChanged;
        public Action<InputAction.CallbackContext> OnActionTriggered;

        #endregion


        protected virtual void Awake()
        {
            playerInput = GetComponentInChildren<PlayerInput>(true);
            eventSystem = GetComponentInChildren<EventSystem>(true);

            Initialize();
        }

        protected virtual void Start()
        {
            Register();
            SelectARandomButtonOnStart();
        }

        void OnDestroy()
        {
            Unregister();
            UnsubscribeToEvents();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                PlayerInput.ActivateInput();
            }
            else
            {
                PlayerInput.DeactivateInput();
            }
        }


        #region Official Methods

        /// <summary>
        /// Switches the current Action Map to the specified map if it exists.
        /// Invokes the OnActionMapChanged event after switching.
        /// </summary>
        /// <param name="mapNameOrId">The name or ID of the Action Map to switch to.</param>
        public void SwitchCurrentActionMap(string mapNameOrId)
        {
            if (string.IsNullOrEmpty(mapNameOrId)) return;

            var mapExists = PlayerInput.actions.actionMaps.Any(map => map.name == mapNameOrId);
            if (!mapExists) return;

            PlayerInput.SwitchCurrentActionMap(mapNameOrId);

            OnActionMapChanged?.Invoke(mapNameOrId);
        }

        /// <summary>
        /// Enables or disables a specific Action Map by name, independently of the current Action Map.
        /// This allows multiple Action Maps to be active simultaneously.
        /// Updates the list of currently enabled Action Maps after the change.
        /// </summary>
        /// <param name="mapName">The name of the Action Map to enable or disable.</param>
        /// <param name="state">The desired state to set (Enabled or Disabled).</param>
        public void SetActionMapState(string mapName, ActionState state)
        {
            var actionMap = PlayerInput.actions.FindActionMap(mapName, true);
            if (actionMap == null)
            {
                if (showDebugs) Debug.LogWarning($"[BaseLocalInput] Action Map '{mapName}' not found.");
                return;
            }

            if (state == ActionState.Enabled)
            {
                actionMap.Enable();
                if (showDebugs) Debug.Log($"[BaseLocalInput] Action Map enabled: {mapName}");
            }
            else
            {
                actionMap.Disable();
                if (showDebugs) Debug.Log($"[BaseLocalInput] Action Map disabled: {mapName}");
            }

            OnActionMapChanged.Invoke(mapName);
        }


        /// <summary>
        /// Changes the state of the specified action by enabling or disabling it.
        /// </summary>
        /// <param name="actionName">The name of the action to change.</param>
        /// <param name="state">The desired state for the action (Enabled or Disabled).</param>
        public void ChangeActionState(string actionName, ActionState state)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                if (showDebugs) Debug.Log("[BaseLocalInput] Action name is null or empty. Aborting ChangeActionState.");
                return;
            }

            string normalizedActionName = actionName.Normalize();

            foreach (var action in PlayerInput.actions)
            {
                string normalizedCurrentName = action.name.Normalize();

                if (string.Equals(normalizedCurrentName, normalizedActionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    switch (state)
                    {
                        case ActionState.Enabled:
                            action.Enable();
                            if (showDebugs) Debug.Log($"[BaseLocalInput] Action enabled: {action.name}");
                            break;

                        case ActionState.Disabled:
                            action.Disable();
                            if (showDebugs) Debug.Log($"[BaseLocalInput] Action disabled: {action.name}");
                            break;
                    }
                }
            }

            OnActionsChanged?.Invoke();
        }

        /// <summary>
        /// Changes the state of all actions, except for the specified ignored actions.
        /// </summary>
        /// <param name="state">The desired state to set for the actions (Enabled or Disabled).</param>
        /// <param name="ignoredActions">Optional names of actions to ignore.</param>
        public void ChangeActionsStatesExceptFor(ActionState state, params string[] ignoredActions)
        {
            ChangeActionsStatesExceptFor(state, (IEnumerable<string>)ignoredActions);
        }

        /// <summary>
        /// Changes the state of all actions in active action maps, except for the specified ignored actions.
        /// </summary>
        /// <param name="state">The desired state to set for the actions (Enabled or Disabled).</param>
        /// <param name="ignoredActions">A collection of action names to ignore.</param>
        public void ChangeActionsStatesExceptFor(ActionState state, IEnumerable<string> ignoredActions)
        {
            var ignoredSet = new HashSet<string>(
                ignoredActions?
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name.Normalize())
                ?? Enumerable.Empty<string>()
            );

            var currentEnabledMaps = PlayerInput.actions.Where(map => map.enabled);

            foreach (var map in currentEnabledMaps)
            {
                foreach (var action in map.actionMap.actions)
                {
                    if (ignoredSet.Contains(action.name.Normalize()))
                        continue;

                    if (state == ActionState.Enabled)
                        action.Enable();
                    else
                        action.Disable();
                }
            }

            if (showDebugs)
            {
                Debug.Log(ignoredSet.Any()
                    ? $"[BaseLocalInput] Input action state changed to '{state}'. Ignored {ignoredSet.Count} action(s): {string.Join(", ", ignoredSet)}"
                    : $"[BaseLocalInput] Input action state changed to '{state}'. No actions ignored.");
            }

            OnActionsChanged?.Invoke();
        }


        /// <summary>
        /// Switches the current Control Scheme to the specified scheme if it exists.
        /// Throws an exception if the scheme name is null, empty, or does not exist.
        /// </summary>
        /// <param name="schemeName">The name of the Control Scheme to switch to.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the scheme name is null, empty, or does not exist.
        /// </exception>
        public void SwitchCurrentControlScheme(string schemeName)
        {
            if (string.IsNullOrEmpty(schemeName))
                throw new System.ArgumentException("[BaseLocalInput] ControlScheme name cannot be null or empty.", nameof(schemeName));

            var scheme = PlayerInput.actions.controlSchemes.FirstOrDefault(s => s.name == schemeName);
            if (scheme.name == null)
                throw new System.ArgumentException($"[BaseLocalInput] ControlScheme '{schemeName}' does not exist.", nameof(schemeName));

            PlayerInput.SwitchCurrentControlScheme(schemeName);
        }

        /// <summary>
        /// Sets a new <see cref="PlayerInput"/> reference for this local input handler.
        /// This allows swapping the active input source dynamically at runtime.
        /// </summary>
        /// <param name="newPlayerInput">The new PlayerInput instance to assign.</param>
        public void SetPlayerInput(PlayerInput newPlayerInput)
        {
            if (newPlayerInput == null)
            {
                if (showDebugs) Debug.LogWarning("[BaseLocalInput] Tried to set a null PlayerInput.");
                return;
            }

            playerInput = newPlayerInput;

            if (showDebugs)
            {
                Debug.Log($"[BaseLocalInput] PlayerInput has been set. Current control scheme: '{playerInput.currentControlScheme}'");
            }
        }

        #endregion

        #region Events Handlers

        public void OnControlSchemeChangedHandler(PlayerInput input)
        {
            InputDevice lastDevice = input.currentControlScheme switch
            {
                "Keyboard" => Keyboard.current,
                "Gamepad" => Gamepad.current,
                "Mobile" => MagneticFieldSensor.current,
                _ => null
            };

            UpdateInputCurrentControlScheme();

            OnControlSchemeChanged?.Invoke(this.PlayerInput);

            if (!showDebugs) return;

            if (lastDevice != null)
            {
                Debug.Log($"[BaseLocalInput] Current Control Scheme: {input.currentControlScheme}");
                Debug.Log($"[BaseLocalInput] Last Used Device: {lastDevice.displayName}");
            }
            else
            {
                Debug.Log("[BaseLocalInput] Unknown input device.");
            }
        }


        public void OnActionMapChangedHandler(string mapNameOrId)
        {
            if (!showDebugs) return;

            Debug.Log($"[BaseLocalInput] Current Action Map changed to: '{mapNameOrId}'");
        }

        #endregion

        #region Helpers

        void Initialize()
        {
            SubscribeToEvents();

            ManageLocalInputCurrentActionMap(initialConfig);
            ManageLocalInputActionsStates(initialConfig);

            UpdateInputCurrentControlScheme();
            UpdateEnabledActionMapsList();
            UpdateEnabledActionsList();


            void ManageLocalInputActionsStates(InputActionModificationConfig modifyInputActionState)
            {
                if (!modifyInputActionState.ModifyCurrentMapActions) return;

                ChangeActionsStatesExceptFor(modifyInputActionState.ActionState, modifyInputActionState.IgnoredActions);
            }
            void ManageLocalInputCurrentActionMap(InputActionModificationConfig modifyInputActionState)
            {
                if (!modifyInputActionState.ModifyCurrentActionMap) return;

                switch (modifyInputActionState.ActionMapChangeMode)
                {
                    case ActionMapChangeModeEnum.SwitchActionMap:

                        SwitchCurrentActionMap(modifyInputActionState.ActionMapToSwitch);

                        break;

                    case ActionMapChangeModeEnum.EnableOrDisableActionMaps:

                        if (modifyInputActionState.ActionsMapsToEnableOrDisable.Length == 0) return;

                        foreach (var action in modifyInputActionState.ActionsMapsToEnableOrDisable)
                        {
                            SetActionMapState(action.ActionMapName, action.ActionMapState);
                        }

                        break;

                }
            }
        }

        void SubscribeToEvents()
        {
            PlayerInput.controlsChangedEvent.AddListener(OnControlSchemeChangedHandler);
            OnActionMapChanged += OnActionMapChangedHandler;
            OnActionMapChanged += UpdateEnabledActionMapsList;
            OnActionsChanged += UpdateEnabledActionsList;

        }
        void UnsubscribeToEvents()
        {
            PlayerInput.controlsChangedEvent.RemoveListener(OnControlSchemeChangedHandler);
            OnActionMapChanged -= OnActionMapChangedHandler;
            OnActionMapChanged -= UpdateEnabledActionMapsList;
            OnActionsChanged -= UpdateEnabledActionsList;
        }

        private void UpdateEnabledActionMapsList(string mapName = default)
        {
            currentlyEnabledActionMaps.Clear();
            foreach (var map in PlayerInput.actions.actionMaps.Where(map => map.enabled))
            {
                currentlyEnabledActionMaps.Add(map.name);
            }
        }
        private void UpdateEnabledActionsList()
        {
            currentlyEnabledActions.Clear();
            foreach (var action in PlayerInput.actions.Where(action => action.enabled))
            {
                currentlyEnabledActions.Add(action.name);
            }
        }
        private void UpdateInputCurrentControlScheme()
        {
            currentControlScheme = PlayerInput.currentControlScheme;
        }

        private void Register()
        {
            LocalInputManager.Instance.RegisterInput(this);
        }
        private void Unregister()
        {
            LocalInputManager.Instance.UnregisterInput(this);
        }


        private void SelectARandomButtonOnStart()
        {
            if (UIPanelsManager.Instance == null) return;

            var selectable = UIPanelsManager.Instance.CurrentPanel?.gameObject.GetComponentInChildren<Selectable>();

            EventSystem.SetSelectedGameObject(selectable?.gameObject);
        }

        #endregion
    }



    public enum ActionState
    {
        Disabled,
        Enabled
    }

    [System.Serializable]
    public struct ActionMapConfiguration
    {
        public string ActionMapName;
        public ActionState ActionMapState;
    }

    [System.Serializable]
    public struct InputActionModificationConfig
    {
        public bool ModifyCurrentActionMap;
        public ActionMapChangeModeEnum ActionMapChangeMode;

        public bool ModifyCurrentMapActions;
        public ActionState ActionState;
        public string[] IgnoredActions;

        public string ActionMapToSwitch;

        public ActionMapConfiguration[] ActionsMapsToEnableOrDisable;
    }

    public enum ActionMapChangeModeEnum
    {
        SwitchActionMap,
        EnableOrDisableActionMaps,
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InputActionModificationConfig))]
    public class InputActionModificationConfigDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect currentPosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.LabelField(currentPosition, "Modify Current Action Map", EditorStyles.boldLabel);
                currentPosition.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(currentPosition, property.FindPropertyRelative("ModifyCurrentActionMap"));
                currentPosition.y += EditorGUIUtility.singleLineHeight + 2;

                var modeProp = property.FindPropertyRelative("ActionMapChangeMode");
                EditorGUI.PropertyField(currentPosition, modeProp);
                currentPosition.y += EditorGUIUtility.singleLineHeight + 2;

                var mode = (ActionMapChangeModeEnum)modeProp.enumValueIndex;

                if (mode == ActionMapChangeModeEnum.SwitchActionMap)
                {
                    EditorGUI.PropertyField(currentPosition, property.FindPropertyRelative("ActionMapToSwitch"));
                    currentPosition.y += EditorGUIUtility.singleLineHeight + 2;
                }
                else if (mode == ActionMapChangeModeEnum.EnableOrDisableActionMaps)
                {
                    var arrayProp = property.FindPropertyRelative("ActionsMapsToEnableOrDisable");
                    EditorGUI.PropertyField(currentPosition, arrayProp, true);
                    currentPosition.y += EditorGUI.GetPropertyHeight(arrayProp, true) + 2;
                }

                currentPosition.y += EditorGUIUtility.singleLineHeight + 10;

                EditorGUI.LabelField(currentPosition, "Modify Current Actions", EditorStyles.boldLabel);
                currentPosition.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(currentPosition, property.FindPropertyRelative("ModifyCurrentMapActions"));
                currentPosition.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(currentPosition, property.FindPropertyRelative("ActionState"));
                currentPosition.y += EditorGUIUtility.singleLineHeight + 2;

                var ignoredActionsArrayProp = property.FindPropertyRelative("IgnoredActions");
                EditorGUI.PropertyField(currentPosition, ignoredActionsArrayProp, true);
                currentPosition.y += EditorGUI.GetPropertyHeight(ignoredActionsArrayProp, true) + 2;

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float height = 0f;

            height += EditorGUIUtility.singleLineHeight + 2;

            height += EditorGUIUtility.singleLineHeight + 2;

            height += EditorGUIUtility.singleLineHeight + 2;
            height += EditorGUIUtility.singleLineHeight + 2;

            var modeProp = property.FindPropertyRelative("ActionMapChangeMode");
            var mode = (ActionMapChangeModeEnum)modeProp.enumValueIndex;

            if (mode == ActionMapChangeModeEnum.SwitchActionMap)
            {
                height += EditorGUIUtility.singleLineHeight + 2;
            }
            else if (mode == ActionMapChangeModeEnum.EnableOrDisableActionMaps)
            {
                var arrayProp = property.FindPropertyRelative("ActionsMapsToEnableOrDisable");
                height += EditorGUI.GetPropertyHeight(arrayProp, true) + 2;
            }

            height += EditorGUIUtility.singleLineHeight + 30;

            height += EditorGUIUtility.singleLineHeight + 2;
            height += EditorGUIUtility.singleLineHeight + 2;

            var ignoredActionsProp = property.FindPropertyRelative("IgnoredActions");
            height += EditorGUI.GetPropertyHeight(ignoredActionsProp, true) + 2;

            return height;
        }
    }
#endif
}
