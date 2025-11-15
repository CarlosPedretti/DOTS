using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using QFSW.QC;
using Utilities.UI;
using DG.Tweening;

namespace Utilities
{
    [RequireComponent(typeof(UIAnimator))]
    public class LoadingScreenManager : Singleton<LoadingScreenManager>
    {
        [Header("UI References")]
        [SerializeField] private GameObject loadingScreenPanel;
        [SerializeField] private Image loadingImage;
        [SerializeField] private TMP_Text loadingTipText;
        [SerializeField] private Slider progressBar;

        [Header("Animations")]
        [SerializeField] private UIAnimationRoutine showAnimationRoutine;
        [SerializeField] private UIAnimationRoutine hideAnimationRoutine;
        private UIAnimator animator;



        [Header("Loading Images")]
        [SerializeField] private Sprite[] loadingSprites;
        [Min(1)]
        [SerializeField] private float imageChangeInterval = 5f;
        [SerializeField] private bool useImageFade = true;
        [SerializeField] private float imageFadeDuration = 0.5f;

        [Header("Tips")]
        [TextArea]
        [SerializeField] private string[] tips;
        [Min(1)]
        [SerializeField] private float tipChangeInterval = 5f;
        [SerializeField] private bool useTipFade = true;
        [SerializeField] private float tipFadeDuration = 0.5f;

        private Coroutine tipsCoroutine;
        private Coroutine imagesCoroutine;
        private int lastTipIndex = -1;
        private int lastImageIndex = -1;

        protected override void Awake()
        {
            base.Awake();

            animator = GetComponent<UIAnimator>();

            if (loadingScreenPanel != null)
                loadingScreenPanel.SetActive(false);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        [Command]
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        [Command]
        public void UnloadScene(string sceneName)
        {
            StartCoroutine(UnloadSceneAsync(sceneName));
        }

        #region Manual Control (Show/Hide Loading Screen)

        /// <summary>
        /// Shows the loading screen with an optional tip. 
        /// Does not modify the progress bar or load any scenes.
        /// </summary>
        public void ShowLoadingScreen(string customTip = null)
        {
            if (progressBar != null)
                progressBar.gameObject.SetActive(false);

            HandleShowAnimation();

            if (!string.IsNullOrEmpty(customTip))
            {
                loadingTipText.text = customTip;
            }
            else if (tips.Length > 0)
            {
                int newTipIndex = GetRandomIndexDifferentFrom(lastTipIndex, tips.Length);
                lastTipIndex = newTipIndex;
                loadingTipText.text = tips[newTipIndex];
            }

            if (loadingSprites.Length > 0)
                imagesCoroutine = StartCoroutine(ChangeImageRoutine());
        }

        /// <summary>
        /// Hides the loading screen with its corresponding animation.
        /// </summary>
        public void HideLoadingScreen()
        {
            StopLoadingRoutines();
            HandleHideAnimation();
        }

        #endregion


        private IEnumerator LoadSceneAsync(string sceneName)
        {
            HandleShowAnimation();

            if (progressBar != null)
                progressBar.gameObject.SetActive(true);

            if (tips.Length > 0)
                tipsCoroutine = StartCoroutine(ChangeTipsRoutine());

            if (loadingSprites.Length > 0)
                imagesCoroutine = StartCoroutine(ChangeImageRoutine());

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                if (progressBar != null)
                    progressBar.value = progress;

                if (operation.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(0.5f);
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }

            StopLoadingRoutines();
            HandleHideAnimation();
        }

        private IEnumerator UnloadSceneAsync(string sceneName)
        {
            loadingScreenPanel.SetActive(true);

            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

            while (!operation.isDone)
            {
                if (progressBar != null)
                    progressBar.value = operation.progress;

                yield return null;
            }

            StopLoadingRoutines();
            loadingScreenPanel.SetActive(false);
        }

        private IEnumerator ChangeTipsRoutine()
        {
            loadingTipText.alpha = 1f;

            while (loadingScreenPanel.activeSelf)
            {
                if (useTipFade)
                {
                    // FadeOut
                    yield return loadingTipText.DOFade(0f, tipFadeDuration).WaitForCompletion();

                    int newTipIndex = GetRandomIndexDifferentFrom(lastTipIndex, tips.Length);
                    lastTipIndex = newTipIndex;
                    loadingTipText.text = tips[newTipIndex];

                    // FadeIn
                    yield return loadingTipText.DOFade(1f, tipFadeDuration).WaitForCompletion();
                }
                else
                {
                    int newTipIndex = GetRandomIndexDifferentFrom(lastTipIndex, tips.Length);
                    lastTipIndex = newTipIndex;
                    loadingTipText.text = tips[newTipIndex];
                }

                yield return new WaitForSeconds(tipChangeInterval);
            }
        }

        private IEnumerator ChangeImageRoutine()
        {
            loadingImage.color = new Color(loadingImage.color.r, loadingImage.color.g, loadingImage.color.b, 1f);

            while (loadingScreenPanel.activeSelf)
            {
                if (useImageFade)
                {
                    // FadeOut
                    yield return loadingImage.DOFade(0f, imageFadeDuration).WaitForCompletion();

                    int newImageIndex = GetRandomIndexDifferentFrom(lastImageIndex, loadingSprites.Length);
                    lastImageIndex = newImageIndex;
                    loadingImage.sprite = loadingSprites[newImageIndex];

                    // FadeIn
                    yield return loadingImage.DOFade(1f, imageFadeDuration).WaitForCompletion();
                }
                else
                {
                    int newImageIndex = GetRandomIndexDifferentFrom(lastImageIndex, loadingSprites.Length);
                    lastImageIndex = newImageIndex;
                    loadingImage.sprite = loadingSprites[newImageIndex];
                }

                yield return new WaitForSeconds(imageChangeInterval);
            }
        }

        private void StopLoadingRoutines()
        {
            if (tipsCoroutine != null)
                StopCoroutine(tipsCoroutine);
            if (imagesCoroutine != null)
                StopCoroutine(imagesCoroutine);

            tipsCoroutine = null;
            imagesCoroutine = null;
        }

        private int GetRandomIndexDifferentFrom(int lastIndex, int length)
        {
            if (length <= 1) return 0;

            int newIndex = Random.Range(0, length - 1);

            if (newIndex >= lastIndex)
                newIndex++;

            return newIndex;
        }

        private void UnloadAllOtherScenes(string exceptScene)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded && scene.name != exceptScene)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UnloadAllOtherScenes(scene.name);
        }


        #region Animation Management

        private void HandleShowAnimation()
        {
            if (animator == null || showAnimationRoutine == null)
            {
                loadingScreenPanel.SetActive(true);
                return;
            }

            loadingScreenPanel.SetActive(true);
            animator.KillActiveTween(resetToInitial: true);
            animator.PlayRoutine(showAnimationRoutine);

            animator.ActiveTween?.OnComplete(() =>
            {

            });
        }

        private void HandleHideAnimation()
        {
            if (animator == null || hideAnimationRoutine == null)
            {
                loadingScreenPanel.SetActive(false);
                return;
            }

            animator.KillActiveTween(resetToInitial: true);
            animator.PlayRoutine(hideAnimationRoutine);

            animator.ActiveTween?.OnComplete(() =>
            {
                loadingScreenPanel.SetActive(false);
            });
        }

        #endregion
    }
}
