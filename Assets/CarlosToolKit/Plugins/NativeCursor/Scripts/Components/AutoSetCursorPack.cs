using UnityEngine;
using UnityEngine.SceneManagement;

namespace Riten.Native.Cursors.Virtual
{
    public class AutoSetCursorPack : MonoBehaviour
    {
        [SerializeField] CursorPack _cursorPack;
        [SerializeField] Camera _camera;

        private CursorPack _lastActivated;

        private void OnEnable()
        {
            if (_cursorPack == null)
                return;

            CheckCamera();

            NativeCursor.SetCursorPack(_cursorPack, _camera);
            NativeCursor.SetCursor(NTCursors.Arrow);
            
            _lastActivated = _cursorPack;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || !enabled) return;
            
            if (_lastActivated != null && _lastActivated != _cursorPack)
            {
                NativeCursor.SetCursorPack(_cursorPack, _camera);
                NativeCursor.SetCursor(NTCursors.Arrow);
                _lastActivated = _cursorPack;
            }
        }

        private void OnDisable()
        {
            if (_lastActivated)
            {
                NativeCursor.ClearCursorPack();
                _lastActivated = null;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void CheckCamera()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CheckCamera();
        }
    }
}