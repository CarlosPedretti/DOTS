using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.UI;

namespace Utilities.Input
{
    /// <summary>
    /// Extension of BaseLocalInput that maps input events
    /// to gameplay-related delegates (e.g., movement, jump, shoot).
    /// Provides an event-driven way of handling specific player inputs.
    /// </summary>
    public class LocalPlayerInput : BaseLocalInput
{
        //Examples
        public event Action OnExample;
        public event Action<Vector2> OnMove;
        public event Action OnJump;
        public event Action<bool> OnShoot;
        public event Action<Vector2> OnLook;
        public event Action OnPause;

        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
        }

        #region Input Handlers Examples

        public void OnExampleInput(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            Debug.Log("Example input triggered");

            OnExample?.Invoke();
            OnActionTriggered?.Invoke(ctx);
        }

        //Movement (Vector2)
        public void OnMoveInput(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            Vector2 movement = ctx.ReadValue<Vector2>();
            OnMove?.Invoke(movement);
            OnActionTriggered?.Invoke(ctx);
        }

        //Jump (Botón)
        public void OnJumpInput(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            Debug.Log("Jump pressed");
            OnJump?.Invoke();
            OnActionTriggered?.Invoke(ctx);
        }

        //Shoot (Started/Released)
        public void OnShootInput(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                Debug.Log("Shooting started");
                OnShoot?.Invoke(true);
            }
            else if (ctx.canceled)
            {
                Debug.Log("Shooting stopped");
                OnShoot?.Invoke(false);
            }

            OnActionTriggered?.Invoke(ctx);
        }

        //Look (Vector2)
        public void OnLookInput(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            Vector2 lookDelta = ctx.ReadValue<Vector2>();
            OnLook?.Invoke(lookDelta);
            OnActionTriggered?.Invoke(ctx);
        }

        //Pause (Toggle)
        public void OnPauseInput(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            Debug.Log("Pause pressed");
            OnPause?.Invoke();
            OnActionTriggered?.Invoke(ctx);
        }

        public void OnEsc(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            UIPanelsManager.Instance.SelectPreviousPanel();
        }

        #endregion
    }
}

