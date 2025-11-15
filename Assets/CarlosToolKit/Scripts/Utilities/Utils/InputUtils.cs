using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Utilities.Input
{
    public static class InputUtils
    {
        public static void GetPlayerIDPressingButton(out int playerIDPressingButton)
        {
            playerIDPressingButton = -1;

            var es = EventSystem.current;

            if (es != null)
            {
                var playerInput = es.GetComponentInParent<PlayerInput>();
                playerIDPressingButton = playerInput.playerIndex;
            }
            else
            {
                Debug.LogWarning("No EventSystem found in the scene.");
                playerIDPressingButton = -1;
            }
        }
    }
}