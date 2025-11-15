using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Utilities.UI;

namespace Utilities
{
    public static class Utils
    {
        private static Dictionary<object, Tween> tweens = new();

        private static string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static char GetRandomCharacter()
        {
            int randomIndex = UnityEngine.Random.Range(0, characters.Length);
            return characters[randomIndex];
        }

        public static string GetRandomCharacters(int length)
        {
            char[] randomCharacters = new char[length];
            for (int i = 0; i < length; i++)
            {
                randomCharacters[i] = GetRandomCharacter();
            }
            return new string(randomCharacters);
        }

        public static string GetRandomName()
        {
            int randomIndex = UnityEngine.Random.Range(2000, 20000);
            string randomName = $"User " + randomIndex.ToString();
            return randomName;
        }

        public static string GetRandomServerName()
        {
            char[] stringChars = new char[5];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = characters.ToUpper()[UnityEngine.Random.Range(0, characters.Length)];
            }

            string randomName = new string(stringChars);

            return randomName;
        }

        public static void CheckAndAssignIfNull<T>(ref T obj) where T : UnityEngine.Object
        {
            if (obj == null)
            {
                T[] objects = Resources.FindObjectsOfTypeAll<T>();
                foreach (T foundObj in objects)
                {
                    if (foundObj is Component component)
                    {
                        Debug.Log($"Found Component: {component.gameObject.name} in scene {component.gameObject.scene.name}");
                        if (component.gameObject.scene.isLoaded)
                        {
                            obj = foundObj;
                            break;
                        }
                    }
                    else if (foundObj is GameObject gameObject)
                    {
                        Debug.Log($"Found GameObject: {gameObject.name} in scene {gameObject.scene.name}");
                        if (gameObject.scene.isLoaded)
                        {
                            obj = foundObj;
                            break;
                        }
                    }
                }

                if (obj == null)
                {
                    Debug.LogWarning($"No object of type {typeof(T)} found in the active scene.");
                }
                else
                {
                    Debug.Log($"Assigned {typeof(T)} from the active scene to the reference.");
                }
            }
        }

        public static Component GetComponentByName(GameObject gameObject, string componentName)
        {
            System.Type componentType = System.Type.GetType(componentName);

            if (componentType == null)
            {
                Debug.LogError($"The 'Type' was not found for the component: '{componentName}'");
                return null;
            }

            Component component = gameObject.GetComponent(componentType);

            if (component == null)
            {
                Debug.LogWarning($"The GameOcject '{gameObject.name}' does not have a component of type: '{componentName}'");
            }

            return component;
        }

        public static void CopyText<T>(T obj) where T : Component
        {
            string textToCopy = null;

            if (obj is Text uiText)
            {
                textToCopy = uiText.text;
            }
            else if (obj is TextMesh textMesh)
            {
                textToCopy = textMesh.text;
            }
            else if (obj is InputField inputField)
            {
                textToCopy = inputField.text;
            }
            else if (obj is TMP_InputField tmpInputField)
            {
                textToCopy = tmpInputField.text;
            }
            else if (obj is TMP_Text tmpText)
            {
                textToCopy = tmpText.text;
            }
            else if (obj is Button button)
            {
                textToCopy = button.GetComponentInChildren<Text>()?.text;
            }
            else if (obj is UIButton uiButton)
            {
                textToCopy = uiButton.GetComponentInChildren<Text>()?.text;
            }
            else
            {
                Debug.LogWarning($"CopyText not supported for type '{typeof(T)}'");
            }

            if (textToCopy != null)
            {
                GUIUtility.systemCopyBuffer = textToCopy;
            }
            else
            {
                Debug.LogWarning($"There is no text to copy from: '{typeof(T)}'");
            }
        }

        public static void Fade<T>(T target, float duration, EFadeDirection fadeDirection = EFadeDirection.FadeOut, float delay = 0f)
        {
            if (tweens.TryGetValue(target, out var oldTween))
            {
                oldTween.Kill();
                tweens.Remove(target);
            }

            float startAlpha = fadeDirection == EFadeDirection.FadeIn ? 0f : 1f;
            float endAlpha = fadeDirection == EFadeDirection.FadeIn ? 1f : 0f;

            Tween newTween = null;

            switch (target)
            {
                case CanvasGroup canvasGroup:
                    canvasGroup.alpha = startAlpha;
                    newTween = canvasGroup
                        .DOFade(endAlpha, duration)
                        .SetDelay(delay)
                        .OnComplete(() => tweens.Remove(target));
                    break;

                case SpriteRenderer spriteRenderer:
                    var spriteColor = spriteRenderer.color;
                    spriteColor.a = startAlpha;
                    spriteRenderer.color = spriteColor;
                    newTween = spriteRenderer
                        .DOFade(endAlpha, duration)
                        .SetDelay(delay)
                        .OnComplete(() => tweens.Remove(target));
                    break;

                case Renderer renderer:
                    foreach (var mat in renderer.materials)
                    {
                        var matColor = mat.color;
                        matColor.a = startAlpha;
                        mat.color = matColor;
                        newTween = mat
                            .DOFade(endAlpha, duration)
                            .SetDelay(delay)
                            .OnComplete(() => tweens.Remove(target));
                    }
                    break;

                case Material material:
                    var mColor = material.color;
                    mColor.a = startAlpha;
                    material.color = mColor;
                    newTween = material
                        .DOFade(endAlpha, duration)
                        .SetDelay(delay)
                        .OnComplete(() => tweens.Remove(target));
                    break;

                case UnityEngine.UI.Image uiImage:
                    var imageColor = uiImage.color;
                    imageColor.a = startAlpha;
                    uiImage.color = imageColor;
                    newTween = uiImage
                        .DOFade(endAlpha, duration)
                        .SetDelay(delay)
                        .OnComplete(() => tweens.Remove(target));
                    break;

                case UnityEngine.UI.Text uiText:
                    var textColor = uiText.color;
                    textColor.a = startAlpha;
                    uiText.color = textColor;
                    newTween = uiText
                        .DOFade(endAlpha, duration)
                        .SetDelay(delay)
                        .OnComplete(() => tweens.Remove(target));
                    break;

                case TMPro.TextMeshProUGUI tmpText:
                    var tmpColor = tmpText.color;
                    tmpColor.a = startAlpha;
                    tmpText.color = tmpColor;
                    newTween = tmpText
                        .DOFade(endAlpha, duration)
                        .SetDelay(delay)
                        .OnComplete(() => tweens.Remove(target));
                    break;

                case Light light:
                    float startIntensity = fadeDirection == EFadeDirection.FadeIn ? 0f : light.intensity;
                    float endIntensity = fadeDirection == EFadeDirection.FadeIn ? light.intensity : 0f;
                    light.intensity = startIntensity;
                    newTween = light
                        .DOIntensity(endIntensity, duration)
                        .SetDelay(delay)
                        .OnComplete(() => tweens.Remove(target));
                    break;

                default:
                    Debug.LogWarning($"Fade has no support for type '{typeof(T)}'");
                    return;
            }

            if (newTween != null)
            {
                tweens[target] = newTween;
            }
        }

        public static void StopFade<T>(T target)
        {
            if (tweens.TryGetValue(target, out var tween))
            {
                tween.Kill();
                tweens.Remove(target);
            }
        }

        public static void OpenURL(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
            else
            {
                Debug.LogWarning("The URL used is null or empty");
            }
        }

        public static void QuitApplication()
        {
            Application.Quit();
        }

        public static void DisplayNotification<T>(T message, NotificationType notificationType = NotificationType.Notification)
        {
            //UIPanelsManager.Instance.SelectPanel("Notification");
            //UIPanelsManager.Instance.notificationsPanel.OpenAs(notificationType);
            //UIPanelsManager.Instance.notificationsPanel.DisplayNotification(message, notificationType);
        }

        public static void DisplayConfirmation<T>(T message, Action confirmAction, Action cancelAction)
        {
            //UIPanelsManager.Instance.SelectPanel("Notification");
            //UIPanelsManager.Instance.notificationsPanel.OpenAs(NotificationType.Confirmation);
            //UIPanelsManager.Instance.notificationsPanel.DisplayNotification(message, NotificationType.Confirmation);
            //UIPanelsManager.Instance.notificationsPanel.SetConfirmationActions(confirmAction, cancelAction);
        }

    }
}



