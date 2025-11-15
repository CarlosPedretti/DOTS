using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities.Input
{
    /// <summary>
    /// Singleton manager that keeps track of all active BaseLocalInput instances.
    /// Allows registering/unregistering inputs and executing actions across all players.
    /// </summary>
    [RequireComponent(typeof(PlayerInputManager))]
    public class LocalInputManager : Singleton<LocalInputManager>
    {
        public event Action<BaseLocalInput> OnInputRegistered;
        public event Action<BaseLocalInput> OnInputUnregistered;

        public event Action<BaseLocalInput, InputAction> OnAnyInputActionPerformed;

        public Dictionary<int, BaseLocalInput>.ValueCollection RegisteredInputs { get { return registeredInputs.Values; } }
        private Dictionary<int, BaseLocalInput> registeredInputs = new();

        private PlayerInputManager PlayerInputManager;


        /// <summary>
        /// Direct reference to the BaseLocalInput with player index 0, if available.
        /// </summary>
        public BaseLocalInput MainInput { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            PlayerInputManager = GetComponent<PlayerInputManager>();

            PlayerInputManager.playerJoinedEvent.AddListener(HandlePlayerInputCreation);
            PlayerInputManager.playerLeftEvent.AddListener(HandlePlayerRemoval);

            SetReady();
        }

        protected override void Start()
        {
            if (PlayerInputManager.playerCount == 0)
                PlayerInputManager.JoinPlayer();
        }

        protected void OnDestroy()
        {
            PlayerInputManager.playerJoinedEvent.RemoveListener(HandlePlayerInputCreation);
            PlayerInputManager.playerLeftEvent.RemoveListener(HandlePlayerRemoval);
        }

        /// <summary>
        /// Registers a new BaseLocalInput instance into the manager.
        /// </summary>
        public void RegisterInput(BaseLocalInput input)
        {
            if (input == null) return;
            if (!registeredInputs.ContainsKey(input.PlayerInput.playerIndex))
            {
                if (input == null || input.PlayerInput == null)
                {
                    Debug.LogWarning("[LocalInputManager] Tried to register null input or missing PlayerInput.");
                    return;
                }

                registeredInputs.Add(input.PlayerInput.playerIndex, input);

                if (input.PlayerInput != null && input.PlayerInput.playerIndex == 0)
                {
                    MainInput = input;
                }

                SubscribeToInputActions(input);

                Debug.Log($"[LocalInputManager] Input registered (PlayerIndex: '{input.PlayerInput?.playerIndex}').");

                OnInputRegistered?.Invoke(input);
            }
        }

        /// <summary>
        /// Unregisters a BaseLocalInput instance from the manager.
        /// </summary>
        public void UnregisterInput(BaseLocalInput input)
        {
            if (input == null) return;
            if (registeredInputs.ContainsKey(input.PlayerInput.playerIndex))
            {
                UnsubscribeFromInputActions(input);

                registeredInputs.Remove(input.PlayerInput.playerIndex);

                // Clear main input if player 0 was removed
                if (MainInput == input)
                {
                    MainInput = null;
                }

                Debug.Log($"[LocalInputManager] Input unregistered (PlayerIndex: '{input.PlayerInput?.playerIndex}').");

                OnInputUnregistered?.Invoke(input);
            }
        }

        /// <summary>
        /// Invokes the OnInputRegistered event for all currently registered inputs.
        /// This ensures that any listeners added after inputs were registered
        /// are properly notified of existing players.
        /// </summary>
        public void SyncExistingInputs()
        {
            foreach (var input in registeredInputs.Values)
            {
                OnInputRegistered?.Invoke(input);
            }
        }

        /// <summary>
        /// Executes an action on all registered BaseLocalInput instances.
        /// </summary>
        public void ForEachInput(Action<BaseLocalInput> action)
        {
            if (action == null) return;
            foreach (var input in registeredInputs)
            {
                action.Invoke(input.Value);
            }
        }

        /// <summary>
        /// Finds a BaseLocalInput by its associated PlayerInput index.
        /// </summary>
        public BaseLocalInput GetInputByPlayerIndex(int index)
        {
            registeredInputs.TryGetValue(index, out var input);
            return input;
        }


        #region Players Handlers

        private void HandlePlayerInputCreation(PlayerInput playerInput)
        {
            LocalPlayerInput localPlayerInput = playerInput.GetComponent<LocalPlayerInput>();

            if (registeredInputs.ContainsKey(playerInput.playerIndex))
            {
                Debug.Log("[LocalInputManager] Input with index " + playerInput.playerIndex + " already created.");
                return;
            }

            Debug.Log($"[LocalInputManager] Input created '{playerInput.playerIndex}'");

            if (playerInput != null)
            {
                playerInput.transform.SetParent(transform);
                playerInput.gameObject.name = "LocalPlayerInput_" + playerInput.playerIndex;
            }

            RegisterInput(localPlayerInput);
        }

        private void HandlePlayerRemoval(PlayerInput playerInput)
        {
            if (registeredInputs.ContainsKey(playerInput.playerIndex))
            {
                registeredInputs.Remove(playerInput.playerIndex);

                Debug.LogWarning("[LocalInputManager] Input removed " + playerInput.playerIndex);
            }
        }

        #endregion

        #region Extras

        private void SubscribeToInputActions(BaseLocalInput input)
        {
            var actions = input.PlayerInput.actions;
            foreach (var map in actions.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    action.performed += ctx => OnAnyInputActionPerformed?.Invoke(input, action);
                }
            }
        }

        private void UnsubscribeFromInputActions(BaseLocalInput input)
        {
            var actions = input.PlayerInput.actions;
            foreach (var map in actions.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    action.performed -= ctx => OnAnyInputActionPerformed?.Invoke(input, action);
                }
            }
        }


        #endregion
    }
}
