using UnityEngine;

namespace Utilities.UI
{
    public class DebugInfoUI : MonoBehaviour
    {
        [SerializeField] float width = 190;
        [SerializeField] float height = 1060;
        [SerializeField] int fontSize = 25;
        [SerializeField] float updateInterval = 0.2f;

        private float deltaTime = 0.0f;
        private bool showPerformance;
        private float nextUpdateTime = 0.0f;
        private string cachedDebugInfo = "";

        void Update()
        {
            deltaTime = Time.deltaTime;

            if (Time.time >= nextUpdateTime)
            {
                nextUpdateTime = Time.time + updateInterval;
                cachedDebugInfo = GetDebugInfo();
            }
        }

        private void OnGUI()
        {
            if (!showPerformance) return;

            GUIStyle style = new GUIStyle
            {
                fontSize = this.fontSize,
                normal = { textColor = Color.white }
            };

            float x = Screen.width - width - 10;
            float y = 10;

            GUI.Label(new Rect(x, y, width, height), cachedDebugInfo, style);
        }

        private string GetDebugInfo()
        {
            int fps = Mathf.RoundToInt(1.0f / deltaTime);

            return $"{fps} FPS";
        }

        public void ShowPerformance(bool value)
        {
            showPerformance = value;
            Debug.Log($"ShowPerformance {value}");
        }
    }
}

