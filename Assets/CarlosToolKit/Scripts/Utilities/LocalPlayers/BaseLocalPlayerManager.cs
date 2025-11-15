using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Input;

namespace Utilities
{
    public partial class LocalPlayersManager : Singleton<LocalPlayersManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            yield return new WaitUntil(() => LocalInputManager.Instance.IsReady);

            SceneManager.sceneLoaded += OnSceneLoaded;

            LocalInputManager.Instance.OnInputRegistered += HandlePlayerJoin;
            LocalInputManager.Instance.OnInputUnregistered += HandlePlayerLeave;
            Initialize();
            LocalInputManager.Instance.SyncExistingInputs();

            SetReady();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            LocalInputManager.Instance.OnInputRegistered -= HandlePlayerJoin;
            LocalInputManager.Instance.OnInputUnregistered -= HandlePlayerLeave;
        }

        #region LocalPlayerManager

        [SerializeField] int MaxPlayers = 100;

        public event Action<LocalPlayer> OnPlayerJoined;
        public event Action<LocalPlayer> OnPlayerLeft;
        public event Action OnPlayersChanged;
        public LocalPlayer MainPlayer
        {
            get
            {
                if (localPlayers.Count > 0)
                {
                    return localPlayers[0];
                }
                return null;
            }
        }

        private Dictionary<int, LocalPlayer> localPlayers = new();

        public Dictionary<int, LocalPlayer>.ValueCollection LocalPlayers { get { return localPlayers.Values; } }

        public void HandlePlayerJoin(BaseLocalInput localInput)
        {
            LocalPlayerInput localPlayerInput = localInput.GetComponent<LocalPlayerInput>();

            if (localPlayers.Count >= MaxPlayers)
            {
                Debug.LogWarning("[LocalPlayerManager] A new player tried to enter, but the game is full");
                return;
            }

            if (localPlayers.ContainsKey(localPlayerInput.PlayerIndex))
            {
                Debug.Log("[LocalPlayerManager] Player with index " + localPlayerInput.PlayerIndex + " already joined.");
                return;
            }

            Debug.Log("[LocalPlayerManager] Player joined " + localPlayerInput.PlayerIndex);

            LocalPlayer localPlayer = new LocalPlayer(localPlayerInput);
            localPlayers.Add(localPlayer.ID, localPlayer);

            OnPlayerJoined?.Invoke(localPlayer);

            OnPlayersChanged?.Invoke();
        }

        public void HandlePlayerLeave(BaseLocalInput localInput)
        {
            if (localPlayers.ContainsKey(localInput.PlayerIndex))
            {
                var playerWhoLeft = GetLocalPlayerByID(localInput.PlayerIndex);

                Debug.LogWarning("[LocalPlayerManager] Player left " + localInput.PlayerIndex);

                localPlayers.Remove(localInput.PlayerIndex);

                OnPlayerLeft?.Invoke(playerWhoLeft);

                OnPlayersChanged?.Invoke();
            }
        }

        public LocalPlayer GetLocalPlayerByID(int id)
        {
            if (localPlayers.ContainsKey(id))
            {
                return localPlayers[id];
            }

            return null;
        }

        public List<LocalPlayer> GetLocalPlayersList()
        {
            return new List<LocalPlayer>(localPlayers.Values);
        }

        public bool AreAllPlayersReady()
        {
            foreach (var player in localPlayers.Values)
            {
                if (!player.IsReady)
                {
                    return false;
                }
            }

            return true;
        }

        [Command]
        public void ListPlayers()
        {
            Debug.Log($"[LocalPlayerManager] Listing {localPlayers.Count} players:");
            foreach (var player in localPlayers.Values)
            {
                Debug.Log($"- Player ID: {player.ID}, IsReady: {player.IsReady}");
            }
        }
        #endregion


        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool isMenuScene = scene.name.ToLower().Contains("menu");

            foreach (var player in localPlayers.Values)
            {
                player.PlayerData.Session.PlayerGameState.Value =
                     isMenuScene ? PlayerGameState.InMenu : PlayerGameState.InGame;
            }
        }
    }

}
