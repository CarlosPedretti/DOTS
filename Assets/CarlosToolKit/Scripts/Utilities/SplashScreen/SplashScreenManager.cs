using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.UI;
using Utilities.Input;

namespace Utilities
{
    public class SplashScreenManager : MonoBehaviour
    {

        [SerializeField] GameObject introCanvas;
        [SerializeField] GameObject mainCanvas;

        [SerializeField] List<SplashScreen> splashScreens = new List<SplashScreen>();

        private static bool hasBeenInitialized = false;


        private void Awake()
        {
            if (hasBeenInitialized)
            {
                introCanvas.SetActive(false);
            }

            if (NewPrefs.GetValue<bool>(ConstPrefsKeys.SHOW_INTRO_KEY) == true || (!NewPrefs.HasKey(ConstPrefsKeys.SHOW_INTRO_KEY)))
            {
                Initialize();
            }
            else
            {
                introCanvas.SetActive(false);
            }
        }

        private void Start()
        {
            LocalInputManager.Instance.MainInput.ChangeActionsStatesExceptFor(Input.ActionState.Disabled);

            Cursor.visible = false;
        }

        private IEnumerator StartOneByOneAllSplashScreens()
        {
            if (splashScreens == null) yield break;

            foreach (var splashScreen in splashScreens)
            {
                if (!splashScreen.gameObject.activeSelf) splashScreen.gameObject.SetActive(true);

                yield return splashScreen.StartCoroutine(splashScreen.StartFade());

                //yield return new WaitForSeconds(splashScreen.FadeEffect.Duration);

                yield return new WaitUntil(() => splashScreen.FadeEffect.IsFinished);

            }

            StartCoroutine(FinalizeIntro());
        }

        private void StartAllAtOnceSplashScreens()
        {
            if (splashScreens == null) return;

            foreach (var splashScreen in splashScreens)
            {
                splashScreen.gameObject.SetActive(true);
                splashScreen.StartCoroutine(splashScreen.StartFade());
            }
        }

        public void FadeAllSplashScreens(EFadeDirection eFadeDirection)
        {
            if (splashScreens == null) return;

            foreach (var splashScreen in splashScreens)
            {
                StartCoroutine(splashScreen.StartFade(eFadeDirection));
            }
        }

        void Initialize()
        {
            if (hasBeenInitialized) return;

            if (splashScreens == null) return;

            mainCanvas.SetActive(false);

            foreach (var splashScreen in splashScreens)
            {
                splashScreen.gameObject.SetActive(false);
            }

            StartCoroutine(StartOneByOneAllSplashScreens());

            hasBeenInitialized = true;
        }

        private IEnumerator FinalizeIntro()
        {
            if (splashScreens == null) yield break;

            FadeAllSplashScreens(EFadeDirection.FadeOut);

            foreach (var splashScreen in splashScreens)
            {
                yield return new WaitUntil(() => splashScreen.FadeEffect.IsFinished);
            }

            yield return new WaitForSeconds(2f);

            introCanvas.SetActive(false);
            mainCanvas.SetActive(true);
        }

    }
}

