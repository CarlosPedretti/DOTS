using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utilities.Input;

namespace Utilities.UI
{
    [RequireComponent(typeof(Outline))]
    public class UIButtonOutlineManager : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler
    {
        [SerializeField] Vector2 outlineEffectDistance = new Vector2(5f, -5f);
        private Outline outline;
        private Button targetButton;

        private readonly HashSet<int> activePlayers = new HashSet<int>();

        private readonly HashSet<int> permanentPlayers = new HashSet<int>();

        private void Awake()
        {
            targetButton = GetComponentInParent<Button>();
            outline = targetButton != null ? targetButton.GetComponent<Outline>() : null;

            if (targetButton == null)
            {
                Debug.LogWarning($"[{nameof(UIButtonOutlineManager)}] Target Button is null on GameObject '{gameObject.name}'.", this);
            }

            if (outline == null)
            {
                Debug.LogWarning($"[{nameof(UIButtonOutlineManager)}] Outline component is missing on GameObject '{gameObject.name}'.", this);
            }

            UpdateOutline();
        }

        #region Pointer and Selection Handlers

        public void OnPointerEnter(PointerEventData eventData)
        {
            SelectByPlayer();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DeselectByPlayer();
        }

        public void OnSelect(BaseEventData eventData)
        {
            SelectByPlayer();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            DeselectByPlayer();
        }

        #endregion

        #region Permanent Player

        /// <summary>
        /// Forces this button to always display the specified player's color,
        /// even when the button loses focus or is deselected.
        /// </summary>
        /// <param name="playerID">The player ID whose color should remain visible.</param>
        public void SetPermanentPlayerColor(int playerID)
        {
            if (permanentPlayers.Add(playerID))
                UpdateOutline();
        }

        /// <summary>
        /// Removes the permanent color of the specified player.
        /// </summary>
        /// <param name="playerID">The player ID whose color should be removed.</param>
        public void RemovePermanentPlayerColor(int playerID)
        {
            if (permanentPlayers.Remove(playerID))
                UpdateOutline();
        }

        /// <summary>
        /// Clears all permanent player colors from this button.
        /// </summary>
        public void ClearPermanentColors()
        {
            if (permanentPlayers.Count > 0)
            {
                permanentPlayers.Clear();
                UpdateOutline();
            }
        }

        #endregion

        private void SelectByPlayer()
        {
            InputUtils.GetPlayerIDPressingButton(out int playerID);

            if (!activePlayers.Add(playerID))
                return;

            UpdateOutline();
        }

        private void DeselectByPlayer()
        {
            InputUtils.GetPlayerIDPressingButton(out int playerID);

            if (permanentPlayers.Contains(playerID))
                return;

            if (!activePlayers.Remove(playerID))
                return;

            UpdateOutline();
        }

        private void UpdateOutline()
        {
            HashSet<int> allPlayers = new HashSet<int>(activePlayers);
            allPlayers.UnionWith(permanentPlayers);

            if (allPlayers.Count == 0)
            {
                outline.effectColor = Color.clear;
                return;
            }

            Color mixedColor = Color.black;
            foreach (int playerID in allPlayers)
            {
                mixedColor += GetColorForPlayer(playerID);
            }

            mixedColor /= allPlayers.Count;
            outline.effectColor = mixedColor;
            outline.effectDistance = outlineEffectDistance;
        }

        private Color GetColorForPlayer(int id)
        {
            return id switch
            {
                0 => Color.red,
                1 => Color.blue,
                2 => Color.green,
                3 => Color.yellow,
                4 => Color.cyan,
                5 => Color.magenta,
                6 => new Color(1f, 0.5f, 0f),
                7 => new Color(0.5f, 0f, 1f),
                8 => new Color(1f, 0.75f, 0.8f),
                9 => new Color(0.6f, 0.3f, 0f),
                10 => new Color(0f, 1f, 0.5f),
                11 => new Color(0f, 0.5f, 1f),
                _ => Color.white,
            };
        }
    }
}
