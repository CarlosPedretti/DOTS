using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utilities.UI;

namespace Utilities
{
    //Configuration Manager Example
    public class ConfigurationManager : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        private Resolution[] resolutions;
        private bool hasBeenInitialized = false;

        private void Awake()
        {
            resolutions = SettingsManager.Instance.Resolutions;

            InitializeResolutionDropdown();

            hasBeenInitialized = true;
        }

        public void OnResetToDefaultButton()
        {
            string message = "Do you want to set all the settings to default? Changes will be applied immediately.";
            Utils.DisplayConfirmation<string>(message, OnDefaultConfirm, OnDefaultCancel);
        }

        private void OnDefaultConfirm()
        {
            SettingsManager.Instance.SetAllToDefault();
        }

        private void OnDefaultCancel()
        {

        }

        public void OnSaveButton()
        {
            string message = "Do you want to save the current settings? Changes will be applied immediately.";
            Utils.DisplayConfirmation<string>(message, OnSaveConfirm, OnSaveCancel);
        }

        private void OnSaveConfirm()
        {
            SettingsManager.Instance.SaveUnsavedConfiguratios();
        }

        private void OnSaveCancel()
        {

        }

        private void OnSaveAndCloseButton()
        {
            string message = "You have unsaved changes! Do you want to save the current settings?";
            Utils.DisplayConfirmation<string>(message, OnSaveAndCloseConfirm, OnSaveAndCloseCancel);
        }

        private void OnSaveAndCloseConfirm()
        {
            SettingsManager.Instance.SaveUnsavedConfiguratios();

            UIPanelsManager.Instance.SelectPreviousPanel();
        }

        private void OnSaveAndCloseCancel()
        {
            UIPanelsManager.Instance.SelectPreviousPanel();
        }


        #region Configurations Events

        public void OnMasterSlider(float value)
        {
            SettingsManager.Instance.SetVolume(value, VolumeKeys.MASTER_GROUP_KEY);
        }

        public void OnEffectsSlider(float value)
        {
            SettingsManager.Instance.SetVolume(value, VolumeKeys.SFX_GROUP_KEY);
        }

        public void OnMusicSlider(float value)
        {
            SettingsManager.Instance.SetVolume(value, VolumeKeys.MUSIC_GROUP_KEY);
        }

        public void OnMenuSlider(float value)
        {
            SettingsManager.Instance.SetVolume(value, VolumeKeys.MENU_GROUP_KEY);
        }

        public void OnFullScreenToggle(bool value)
        {
            SettingsManager.Instance.SetFullScreen(value);
        }

        public void OnResolutionDropdown(int value)
        {
            if (!hasBeenInitialized) return;

            SettingsManager.Instance.SetResolution(value);
        }

        public void OnVsyncToggle(bool value)
        {
            SettingsManager.Instance.SetVSync(value);
        }

        public void OnShowIntroToggle(bool value)
        {
            SettingsManager.Instance.ShowIntro(value);
        }

        public void OnPerformanceToggle(bool value)
        {
            SettingsManager.Instance.ShowPerformance(value);
        }

        public void OnFPSTarget(float value)
        {
            SettingsManager.Instance.SetTargetFPS(value);
        }

        public void OnGraphicsDropdown(int value)
        {
            SettingsManager.Instance.SetQuality(value);
        }


        #endregion

        #region Initialization Methods
        private void InitializeResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();

            int currentResolutionIndex = 0;

            List<string> resolutionOptions = new List<string>();

            for (int i = 0; i < resolutions.Length; i++)
            {
                double refreshRate = resolutions[i].refreshRateRatio.value;
                string formattedRefreshRate = refreshRate % 1 == 0 ? refreshRate.ToString("F0") : refreshRate.ToString("F2");

                string resolutionOption = $"{resolutions[i].width} x {resolutions[i].height}, {formattedRefreshRate}Hz";
                resolutionOptions.Add(resolutionOption);

            }

            resolutionDropdown.AddOptions(resolutionOptions);

            currentResolutionIndex = NewPrefs.GetValue<int>(ConstPrefsKeys.RESOLUTION_KEY);

            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();

        }
        #endregion
    }
}