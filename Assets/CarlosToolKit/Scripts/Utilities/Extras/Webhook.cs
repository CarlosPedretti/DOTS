using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using QFSW.QC;

namespace Utilities
{
    public class Webhook : MonoBehaviour
    {
        public const string REPORT_WEB_HOOK_LINK = "WEB_HOOK_URL";
        public const string SUGGESTIONS_WEB_HOOK_LINK = "WEB_HOOK_URL";
        public string Username { get; private set; } = "Set a username in Webhooks";

        [Command]
        public void SendSuggestion(string message)
        {
            MessageFormat messageFormat = new MessageFormat
            {
                content = null,
                username = "Clamnet Bot",
                embeds = new Embed[]
                {
                new Embed
                {
                    title = null,
                    description = message,
                    color = 5814783,
                    author = new Author
                    {
                        name = Username,
                    }
                }
                }
            };

            StartCoroutine(SendMessageCoroutine(messageFormat, SUGGESTIONS_WEB_HOOK_LINK));
        }

        [Command]
        public void SendReportMessage(string header, string message)
        {
            ReportMessageFormat reportMessage = new ReportMessageFormat
            {
                content = null,
                username = "Clamnet Bot",
                embeds = new Embed[]
                {
                new Embed
                {
                    title = header,
                    description = message,
                    color = 5814783,
                    author = new Author
                    {
                        name = Username,
                    }
                }
                }
            };

            StartCoroutine(SendMessageCoroutine(reportMessage, REPORT_WEB_HOOK_LINK));
        }

        private IEnumerator SendMessageCoroutine<T>(T messageContent, string webHookLink)
        {
            string jsonPayload = JsonUtility.ToJson(messageContent);

            using (UnityWebRequest www = new UnityWebRequest(webHookLink, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"An error occurred while sending the message. Error: '{www.error}'");
                }
                else
                {
                    Debug.Log("Message sent successfully.");
                }
            }
        }



        [System.Serializable]
        public class MessageFormat
        {
            public string content;
            public string username;
            public Embed[] embeds;
        }

        [System.Serializable]
        public class ReportMessageFormat
        {
            public string content;
            public string username;
            public Embed[] embeds;
        }

        [System.Serializable]
        public class Embed
        {
            public string title;
            public string description;
            public int color;
            public Author author;
        }

        [System.Serializable]
        public class Author
        {
            public string name;
        }
    }
}

