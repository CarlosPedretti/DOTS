using System;
using Utilities.Input;

namespace Utilities
{
    public partial class LocalPlayer
    {
        public LocalPlayer(LocalPlayerInput localPlayerInput)
        {
            if (localPlayerInput == null)
            {
                throw new ArgumentNullException("localPlayerInput", "LocalPlayerInput cannot be null");
            }

            if (localPlayerInput.PlayerInput == null)
            {
                throw new ArgumentException("PlayerInput component is missing in LocalPlayerInput", "localPlayerInput");
            }

            playerData = new PlayerData();
            LocalPlayerInput = localPlayerInput;
            ID = localPlayerInput.PlayerInput.playerIndex;
        }

        public PlayerData PlayerData { get { return playerData; } }
        private PlayerData playerData;
        public LocalPlayerInput LocalPlayerInput { get; private set; }
        public int ID { get; private set; }
        public bool IsReady { get; private set; }

        public event Action<bool> OnReadyChanged;

        public void SetPlayerReady(bool newBool)
        {
            if (IsReady != newBool)
            {
                IsReady = newBool;
                OnReadyChanged?.Invoke(newBool);
            }
        }
    }
}

