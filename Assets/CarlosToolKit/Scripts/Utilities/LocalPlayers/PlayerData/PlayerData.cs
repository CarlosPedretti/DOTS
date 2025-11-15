using System;
using Utilities.UI;

namespace Utilities
{
    public class PlayerData
    {

        public PersistentPlayerData Persistent = new();
        public SessionPlayerData Session = new();

        public event Action OnModified;

        public PlayerData()
        {
            Persistent = new PersistentPlayerData();
            Session = new SessionPlayerData();

            Persistent.PlayerName.OnChanged += _ => OnModified?.Invoke();
            Persistent.GUID.OnChanged += _ => OnModified?.Invoke();

            Session.PlayerGameState.OnChanged += _ => OnModified?.Invoke();

        }
    }

    public class PersistentPlayerData
    {
        public BindableProperty<string> PlayerName = new();
        public BindableProperty<string> GUID = new();
    }

    public class SessionPlayerData
    {
        public BindableProperty<PlayerGameState> PlayerGameState = new();
        public BindableProperty<bool> IsReady = new();
    }

    public enum PlayerGameState : byte
    {
        InMenu,
        InGame,
    }
}


