using UnityEngine;
using TMPro;
using UnityEngine.UI;
using QFSW.QC;
using System;

namespace Utilities.UI
{
    public class UINotifications : MonoBehaviour
    {
        [SerializeField] TMP_InputField headerInputField;
        [SerializeField] TMP_InputField inputField;
        [SerializeField] UIButton sendButton;
        [SerializeField] UIButton confirmButton;
        [SerializeField] UIButton cancelButton;
        [SerializeField] TextMeshProUGUI headerText;
        [SerializeField] TextMeshProUGUI captionText;
        [SerializeField] Image headerIcon;
        [SerializeField] Image buttonIcon;

        [SerializeField] NotificationType currentNotificationType;

        [SerializeField] Notification notification;
        [SerializeField] Notification infoNotification;
        [SerializeField] Notification warningNotification;
        [SerializeField] Notification errorNotification;
        [SerializeField] Notification reportMessage;
        [SerializeField] Notification suggestionMessage;
        [SerializeField] Notification confirmationMessage;

        [SerializeField] Webhook Webhook;

        private Action onConfirmAction;
        private Action onCancelAction;


        private void OnDisable()
        {
            headerInputField.text = "";
            inputField.text = "";
        }


        private void Awake()
        {
            Initialization();

            if (Webhook == null)
            {
                Webhook = GetComponent<Webhook>();
            }
        }

        #region OnButtons Events

        public void OnCloseButton()
        {
            UIPanelsManager.Instance.SelectPreviousPanel();
        }

        public void OnCopyButton()
        {
            CopyText();
        }

        public void OnSendButton()
        {
            switch (currentNotificationType)
            {
                case NotificationType.Report:
                    SendReport();
                    break;

                case NotificationType.Suggestion:
                    SendSuggestion();
                    break;

            }

            if (CanSend())
            {
                UIPanelsManager.Instance.SelectPreviousPanel();
            }
        }

        public void OnConfirm()
        {
            onConfirmAction?.Invoke();

            onConfirmAction = null;

            UIPanelsManager.Instance.SelectPreviousPanel();
        }

        public void OnCancel()
        {
            onCancelAction?.Invoke();

            onCancelAction = null;

            UIPanelsManager.Instance.SelectPreviousPanel();
        }
        #endregion




        public void DisplayNotification<T>(T message, NotificationType notificationType = NotificationType.Notification)
        {
            if (inputField == null)
            {
                return;
            }

            Notification selectedNotification = NotificationFormatInitialization(notificationType);

            if (message is string)
            {
                selectedNotification.Text = message as string;
            }
            else if (message is System.Exception)
            {
                selectedNotification.Text = (message as System.Exception).Message;
            }
            else
            {
                selectedNotification.Text = message.ToString();
            }

            inputField.text = selectedNotification.Text;
        }

        public void SendReport()
        {
            Notification selectedNotification = NotificationFormatInitialization(NotificationType.Report);

            if (Webhook == null)
            {
                Debug.LogError($"Unable to send a Report. Webhook is null");
                return;
            }

            if (inputField == null || headerInputField == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(inputField.text))
            {
                string error = "There is no message to send.";
                DisplayCaption(error);
                return;
            }

            if (string.IsNullOrEmpty(headerInputField.text))
            {
                string error = "There is no header.";
                DisplayCaption(error);
                return;
            }

            Webhook.SendReportMessage(headerInputField.text.ToUpper(), inputField.text);
        }

        public void SendSuggestion()
        {
            Notification selectedNotification = NotificationFormatInitialization(NotificationType.Suggestion);

            if (Webhook == null)
            {
                Debug.LogError($"Unable to send a Report. Webhook is null");
                return;
            }

            if (inputField == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(inputField.text))
            {
                string error = "There is no message to send.";
                DisplayCaption(error);
                return;
            }


            Webhook.SendSuggestion(inputField.text);
        }

        public void OpenAs(NotificationType notificationType)
        {
            NotificationFormatInitialization(notificationType);

            switch (notificationType)
            {
                case NotificationType.Report:
                    headerInputField.gameObject.SetActive(true);
                    headerInputField.interactable = true;
                    inputField.gameObject.SetActive(true);
                    inputField.interactable = true;
                    sendButton.gameObject.SetActive(true);
                    confirmButton.gameObject.SetActive(false);
                    cancelButton.gameObject.SetActive(false);

                    currentNotificationType = notificationType;
                    break;

                case NotificationType.Suggestion:
                    headerInputField.gameObject.SetActive(false);
                    inputField.gameObject.SetActive(true);
                    inputField.interactable = true;
                    sendButton.gameObject.SetActive(true);
                    confirmButton.gameObject.SetActive(false);
                    cancelButton.gameObject.SetActive(false);

                    currentNotificationType = notificationType;
                    break;

                case NotificationType.Confirmation:

                    headerInputField.gameObject.SetActive(false);
                    inputField.gameObject.SetActive(true);
                    inputField.interactable = false;
                    sendButton.gameObject.SetActive(false);

                    confirmButton.gameObject.SetActive(true);
                    cancelButton.gameObject.SetActive(true);

                    currentNotificationType = notificationType;
                    break;

                default:
                    headerInputField.gameObject.SetActive(false);
                    inputField.gameObject.SetActive(true);
                    inputField.interactable = false;
                    sendButton.gameObject.SetActive(false);
                    confirmButton.gameObject.SetActive(false);
                    cancelButton.gameObject.SetActive(false);

                    currentNotificationType = notificationType;

                    break;
            }
        }

        private Notification NotificationFormatInitialization(NotificationType notificationType)
        {

            Notification selectedNotification = null;

            switch (notificationType)
            {
                case NotificationType.Notification:
                    selectedNotification = notification;
                    break;

                case NotificationType.Info:
                    selectedNotification = infoNotification;
                    break;

                case NotificationType.Warning:
                    selectedNotification = warningNotification;
                    break;

                case NotificationType.Error:
                    selectedNotification = errorNotification;
                    break;

                case NotificationType.Report:
                    selectedNotification = reportMessage;
                    break;

                case NotificationType.Suggestion:
                    selectedNotification = suggestionMessage;
                    break;

                case NotificationType.Confirmation:
                    selectedNotification = confirmationMessage;
                    break;

            }

            headerText.text = selectedNotification.Title.ToUpper();

            headerIcon.sprite = selectedNotification.Icon;
            headerIcon.color = selectedNotification.Color;

            currentNotificationType = selectedNotification.Type;

            return selectedNotification;
        }

        public void SetConfirmationActions(Action confirmAction, Action cancelAction)
        {
            onConfirmAction = confirmAction;
            onCancelAction = cancelAction;
        }



        private void CopyText()
        {
            if (inputField == null)
            {
                return;
            }

            Utils.CopyText(inputField);

            string captionTextFormat = $"'{currentNotificationType}' message copied succesfully.";

            captionText.text = captionTextFormat;

            Color newColor = captionText.color;
            newColor.a = 1f;
            captionText.color = newColor;

            Utils.Fade(captionText, 4);
        }

        private void DisplayCaption(string captionErrorMessage)
        {
            string captionTextFormat = $"{captionErrorMessage}";

            captionText.text = captionTextFormat;

            Color newColor = captionText.color;
            newColor.a = 1f;
            captionText.color = newColor;

            Utils.Fade(captionText, 4);
        }

        private void Initialization()
        {
            inputField.interactable = false;

            Color newColor = captionText.color;
            newColor.a = 0f;
            captionText.color = newColor;
        }

        private bool CanSend()
        {
            if (string.IsNullOrEmpty(inputField.text) || string.IsNullOrEmpty(headerInputField.text))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public enum NotificationType
    {
        Notification,
        Info,
        Warning,
        Error,
        Report,
        Suggestion,
        Confirmation
    }

    [System.Serializable]
    public class Notification
    {
        public NotificationType Type;
        public string Title;
        public string Text;
        public Sprite Icon;
        public Color Color;
    }
}