using QFSW.QC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Utilities.UI;
using Utilities.Localization;

namespace Utilities
{
    public partial class SettingsManager : Singleton<SettingsManager>
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private DebugInfoUI debugInfoUI;

        //Resolution
        public Resolution[] Resolutions { get { return resolutions; } }
        private Resolution[] resolutions;
        private HashSet<(int width, int height)> commonResolutions = new HashSet<(int, int)>
    {
        (1920, 1080), // Full HD
        (1600, 900),  // HD+
        (1366, 768),  // WXGA
        (1280, 720),  // HD
        (1024, 768)   // XGA (4:3)
    };

        protected override void Awake()
        {
            base.Awake();

            GetConfigurationsFromScene();
        }

        protected override void Start()
        {
            base.Start();

            InitializeConfigurations();

            FilterResolutions();

            ExecuteConfigurationActions();

            SetReady();
        }

        #region Settings Methods

        /// <summary>
        /// Sets the volume of an AudioMixer group using a logarithmic scale.
        /// </summary>
        /// <param name="volume">Linear volume value (0.0 to 1.0).</param>
        /// <param name="audioMixerGroupKey">The exposed parameter name in the AudioMixer.</param>
        public void SetVolume(float volume, string audioMixerGroupKey)
        {
            if (audioMixer != null)
            {
                float lerpedVolume = Mathf.Log10(volume) * 20;
                audioMixer.SetFloat(audioMixerGroupKey, lerpedVolume);
            }
        }

        /// <summary>
        /// Sets the screen resolution from a predefined resolutions array.
        /// </summary>
        /// <param name="resolutionIndex">Index of the desired resolution in the array.</param>
        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        /// <summary>
        /// Toggles fullscreen mode.
        /// </summary>
        /// <param name="isFullScreen">True for fullscreen, false for windowed mode.</param>
        [Command]
        public void SetFullScreen(bool isFullScreen)
        {
            Screen.fullScreen = isFullScreen;
        }

        /// <summary>
        /// Sets the target frames per second (FPS) for the application.
        /// </summary>
        /// <param name="fps">Desired FPS value. Use 0 or lower for unlimited.</param>
        public void SetTargetFPS(float fps)
        {
            if (fps > 0)
            {
                Application.targetFrameRate = (int)fps;
            }
            else
            {
                Application.targetFrameRate = -1;
            }
        }

        /// <summary>
        /// Enables or disables vertical synchronization (VSync).
        /// </summary>
        /// <param name="enabled">True to enable VSync, false to disable.</param>
        public void SetVSync(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
        }

        /// <summary>
        /// Sets the graphical quality level.
        /// </summary>
        /// <param name="qualityIndex">Index of the desired quality level.</param>
        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }

        /// <summary>
        /// Stores whether the intro should be shown in player preferences.
        /// </summary>
        /// <param name="showValue">True to show the intro, false to skip.</param>
        public void ShowIntro(bool showValue)
        {
            NewPrefs.SetValue<bool>(ConstPrefsKeys.SHOW_INTRO_KEY, showValue);
        }

        /// <summary>
        /// Toggles the performance debug information UI.
        /// </summary>
        /// <param name="enabled">True to show performance info, false to hide.</param>
        public void ShowPerformance(bool enabled)
        {
            debugInfoUI?.ShowPerformance(enabled);
        }

        /// <summary>
        /// Changes the game language by its index in the localization settings.
        /// </summary>
        /// <param name="index">Index of the desired language.</param>
        public void SetLanguageByIndex(int index)
        {
            StartCoroutine(LocalizationUtils.ChangeLanguageByIndex(index));
        }

        /// <summary>
        /// Changes the game language by its language code.
        /// </summary>
        /// <param name="code">Language code (e.g., "en", "es").</param>
        public void SetLanguageByCode(string code)
        {
            StartCoroutine(LocalizationUtils.ChangeLanguageByCode(code));
        }

        #endregion

        #region Cofiguration Implemantation

        [Tooltip("Add all the configurations in the array in order to make them work")]
        private List<IConfiguration> configurations = new List<IConfiguration>();

        public void RegisterConfiguration(IConfiguration configuration)
        {
            CleanupConfigurations();
            if (configurations.Contains(configuration)) return;
            configurations.Add(configuration);
        }

        public void UnRegisterConfiguration(IConfiguration configuration)
        {
            configurations.Remove(configuration);
            CleanupConfigurations();
        }

        private void CleanupConfigurations()
        {
            configurations.RemoveAll(c => c == null);
        }

        /// <summary>
        /// Saves the current values of all configurations to persistent storage.
        /// Each configuration value is saved using its corresponding key and type.
        /// </summary>
        [Command]
        public void SaveAll()
        {
            if (configurations.Count <= 0) return;

            foreach (var config in configurations)
            {
                if (config == null) continue;

                config.SaveConfiguration();
            }

            NewPrefs.Save();
            Debug.Log("Settings saved!");
        }

        /// <summary>
        /// Saves the current values of the configurations with unsaved changes.
        /// Each configuration value is saved using its corresponding key and type.
        /// </summary>
        [Command]
        public void SaveUnsavedConfiguratios()
        {
            if (configurations.Count <= 0) return;

            foreach (var config in configurations)
            {
                if (config == null) continue;

                if (config.IsModified)
                {
                    config.SaveConfiguration();
                }

            }

            NewPrefs.Save();
            Debug.Log("Settings saved!");
        }

        /// <summary>
        /// Reverts all configurations to their last saved values,
        /// discarding any unsaved changes made by the user.
        /// Only modified configurations will be reverted.
        /// </summary>
        [Command]
        public void RevertToSavedValueAllModifiedConfigurations()
        {
            if (configurations.Count <= 0) return;

            foreach (var config in configurations)
            {
                if (config == null) continue;

                if (config.IsModified)
                {
                    config.RevertToSaved();
                }
            }
        }

        /// <summary>
        /// Resets all configurations to their default values as defined in their respective settings.
        /// </summary>
        [Command]
        public void SetAllToDefault()
        {
            if (configurations.Count <= 0) return;

            foreach (var config in configurations)
            {
                if (config == null) continue;

                config.SetToDefault();
            }
        }


        /// <summary>
        /// Invokes the associated UnityEvent or registered actions of each configuration,
        /// regardless of whether the UI element is active in the hierarchy.
        /// </summary>
        [Command]
        private void ExecuteConfigurationActions()
        {
            if (configurations.Count <= 0) return;

            foreach (var config in configurations)
            {
                if (config == null)
                {
                    Debug.LogWarning("Null configuration found during ExecuteConfigurationActions.");
                }

                config.InvokeEvent();
            }
        }


        /// <summary>
        /// Checks if any Configuration object has unsaved changes.
        /// </summary>
        /// <returns>True if at least one Configuration has unsaved changes; otherwise, false.</returns>
        public bool HasUnsavedChanges()
        {
            foreach (var config in configurations)
            {
                if (config == null) continue;

                if (config.IsModified)
                {
                    Debug.Log($"Unsaved change detected in config: {config.PrefsKey}");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves a Configuration object based on its PlayerPrefs key.
        /// </summary>
        /// <param name="ConfigurationPrefsKey">The PlayerPrefs key associated with the Configuration.</param>
        /// <returns>The matching Configuration if found; otherwise, null.</returns>
        public IConfiguration GetConfiguration(string ConfigurationPrefsKey)
        {
            foreach (var config in configurations)
            {
                if (config == null) continue;

                if (config.PrefsKey.Equals(ConfigurationPrefsKey, StringComparison.OrdinalIgnoreCase))
                {
                    return config;
                }
            }

            return null;
        }

        private void GetConfigurationsFromScene()
        {
            IConfiguration[] normalConfigurations = Resources.FindObjectsOfTypeAll<Utilities.Configuration>();
            IConfiguration[] otherConfigurations = Resources.FindObjectsOfTypeAll<Utilities.ScriptableConfiguration>();

            var allConfigurations = normalConfigurations.Concat(otherConfigurations);

            foreach (var config in allConfigurations)
            {
                RegisterConfiguration(config);
            }
        }

        private void InitializeConfigurations()
        {
            if (configurations.Count <= 0) return;

            foreach (var config in configurations)
            {
                if (config == null)
                {
                    Debug.LogWarning("Null configuration found during initialization.");
                    continue;
                }

                config.Initialize();
            }
        }

        #endregion

        #region Settings Commands

        [Command]
        private void ClearPrefs()
        {
            NewPrefs.DeleteAll();
            NewPrefs.Save();
            Debug.Log("NewPrefs cleared...");
        }

        [Command]
        private void DisplayAllRegisteredConfigurations()
        {
            foreach (var config in configurations)
            {
                if (config == null) continue;
                Debug.Log($"Configuration Key: {config.PrefsKey} | Is Modified: {config.IsModified}");
            }
        }

        #endregion

        #region Auxiliar Methods
        private void FilterResolutions()
        {
            resolutions = (from res in Screen.resolutions
                           where commonResolutions.Contains((res.width, res.height))
                           select res).ToArray();

            if (resolutions.Length == 0)
            {
                Debug.LogError("No valid resolutions found.");
                return;
            }

            int recommendenResolutionIndex = -1;

            if (!NewPrefs.HasKey(ConstPrefsKeys.RESOLUTION_KEY))
            {
                recommendenResolutionIndex = GetRecommendedResolution(resolutions);

                if (recommendenResolutionIndex < 0 || recommendenResolutionIndex >= resolutions.Length)
                {
                    Debug.LogError($"Invalid recommended resolution index: {recommendenResolutionIndex}");
                    return;
                }

                NewPrefs.SetValue<int>(ConstPrefsKeys.RESOLUTION_KEY, recommendenResolutionIndex);

                SetResolution(recommendenResolutionIndex);

                Debug.Log($"recommendenResolutionIndex: {recommendenResolutionIndex}");
            }

            var resolutionConfig = GetConfiguration(ConstPrefsKeys.RESOLUTION_KEY);

            if(resolutionConfig == null)
            {
                Debug.LogWarning("Resolution configuration not found.");
                return;
            }

            resolutionConfig.SetDefaultValue(GetRecommendedResolution(resolutions));

        }

        private int GetRecommendedResolution(Resolution[] resolutionOptions)
        {
            int recommendedResolutionIndex = 0;
            Resolution currentResolution = Screen.currentResolution;
            Resolution recommendedResolution = currentResolution;

            float bestMatchScore = float.MaxValue;

            for (int i = 0; i < resolutionOptions.Length; i++)
            {
                Resolution resolution = resolutionOptions[i];

                float sizeDifference = Mathf.Abs((resolution.width * resolution.height) - (currentResolution.width * currentResolution.height));
                float refreshRateDifference = Mathf.Abs((float)resolution.refreshRateRatio.value - (float)currentResolution.refreshRateRatio.value);

                float matchScore = sizeDifference + (refreshRateDifference * 10);

                if (matchScore < bestMatchScore)
                {
                    bestMatchScore = matchScore;
                    recommendedResolution = resolution;
                    recommendedResolutionIndex = i;
                }
            }

            return recommendedResolutionIndex;
        }
        #endregion

        #region Resolution Commands

        /// <summary>
        /// Displays all available screen resolutions with their corresponding index.
        /// </summary>
        [Command]
        private void DisplayResolutions()
        {
            if (resolutions == null || resolutions.Length == 0)
            {
                Debug.LogWarning("[SettingsManager] No resolutions available to display. Make sure they are initialized.");
                return;
            }

            Debug.Log("------ Available Resolutions ------");
            for (int i = 0; i < resolutions.Length; i++)
            {
                Resolution res = resolutions[i];
                Debug.Log($"Index: {i} | Resolution: {res.width}x{res.height} @ {res.refreshRateRatio.value}Hz");
            }
            Debug.Log("-----------------------------------");
        }

        /// <summary>
        /// Changes the current screen resolution based on the provided index.
        /// </summary>
        /// <param name="index">Index of the desired resolution (use DisplayResolutions to view available indices).</param>
        [Command]
        private void ChangeResolution(int index, bool fullScreen = true)
        {
            SetFullScreen(fullScreen);

            if (resolutions == null || resolutions.Length == 0)
            {
                Debug.LogError("[SettingsManager] Resolutions list is empty or not initialized.");
                return;
            }

            if (index < 0 || index >= resolutions.Length)
            {
                Debug.LogWarning($"[SettingsManager] Invalid resolution index: {index}. Use 'DisplayResolutions' to see valid indices.");
                DisplayResolutions();
                return;
            }

            Resolution selectedRes = resolutions[index];
            SetResolution(index);

            Debug.Log($"[SettingsManager] Resolution changed to {selectedRes.width}x{selectedRes.height} @ {selectedRes.refreshRateRatio.value}Hz (Index: {index})");
        }

        #endregion

        #region Language Commands

        [Command]
        private void DisplayLanguages()
        {
            Debug.Log("------ Available Languages ------");

            for (int i = 0; i < LocalizationUtils.Languages.Count; i++)
            {
                var lang = LocalizationUtils.Languages[i];
                Debug.Log($"Index: {i} | Code: {lang.Code}, Name: {lang.LocaleName}");
            }
            Debug.Log("----------------------------------");
        }

        #endregion

    }
}

