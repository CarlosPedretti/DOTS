using DG.Tweening;
using System.Collections;
using UnityEngine;
using Utilities.UI;

namespace Utilities
{
    [RequireComponent(typeof(Animator))]
    public class SplashScreen : MonoBehaviour
    {
        [Header("Resources")]
        public UIFader FadeEffect;
        public AudioSource SoundSource;
        public AudioClip InSound;
        public AudioClip OutSound;
        Animator objAnimator;

        [Header("Settings")]
        [Range(3, 30)] public float screenTime = 8;
        [Range(0.1f, 1)] public float titleSpeed = 1;
        [Range(1, 10)] public float transitionMultiplier = 4;

        void OnEnable()
        {
            if (objAnimator == null)
                objAnimator = gameObject.GetComponent<Animator>();

            objAnimator.SetFloat("Speed", titleSpeed);
            FadeEffect?.gameObject.SetActive(true);
            StartCoroutine("StartFade");

            if (SoundSource != null && InSound != null)
                SoundSource.PlayOneShot(InSound);
        }

        public IEnumerator StartFade()
        {
            yield return new WaitForSecondsRealtime(screenTime - FadeEffect.Duration * transitionMultiplier);

            FadeEffect?.StartFade();

            if (SoundSource != null && InSound != null)
                SoundSource.PlayOneShot(OutSound);

            StopCoroutine("StartFade");
        }

        public IEnumerator StartFade(EFadeDirection eFadeDirection)
        {
            yield return new WaitForSecondsRealtime(screenTime - FadeEffect.Duration * transitionMultiplier);

            FadeEffect?.StartFade(eFadeDirection);

            if (SoundSource != null && InSound != null)
                SoundSource.PlayOneShot(OutSound);

            StopCoroutine("StartFade");
        }
    }
}

