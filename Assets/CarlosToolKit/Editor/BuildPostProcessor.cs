using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

namespace Utilities
{
    public class BuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private const string BOT_NAME = "Change my name";
        private const string WEBHOOK_URL = "WEB_HOOK_URL";

        public void OnPostprocessBuild(BuildReport report)
        {
            try
            {
                string buildPath = report.summary.outputPath;
                string buildDirectory = Path.GetDirectoryName(buildPath);
                string buildParentDirectory = Directory.GetParent(buildDirectory).FullName;
                string version = Application.version;

                if (!Directory.Exists(buildDirectory))
                {
                    UnityEngine.Debug.LogError("Directory not found: " + buildDirectory);
                    return;
                }

                string zipDirectory = Path.Combine(buildParentDirectory, "BuildZips");
                Directory.CreateDirectory(zipDirectory);

                string platform = report.summary.platform.ToString();
                string zipFileName = $"{Application.productName} - v{version} {platform}.zip";
                string zipPath = Path.Combine(zipDirectory, zipFileName);
                string versionFolderName = $"{Application.productName} - v{version} {platform}";

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                    while (File.Exists(zipPath))
                        System.Threading.Thread.Sleep(100);
                }

                using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                    {
                        AddDirectoryToZip(archive, buildDirectory, buildDirectory, versionFolderName);
                    }
                }

                UnityEngine.Debug.Log($"Build compressed in: {zipPath}");

                SendVersion();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Error: " + e.Message);
            }
        }

        private void AddDirectoryToZip(ZipArchive archive, string rootDir, string sourceDir, string versionFolder)
        {
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                if (file.EndsWith(".zip")) continue;

                string relativePath = Path.GetRelativePath(rootDir, file);
                string entryPath = Path.Combine(versionFolder, relativePath);

                archive.CreateEntryFromFile(file, entryPath);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                if (directory.Contains("BuildZips")) continue;

                string relativeDir = Path.GetRelativePath(rootDir, directory);
                string entryDir = Path.Combine(versionFolder, relativeDir);

                AddDirectoryToZip(archive, rootDir, directory, versionFolder);
            }
        }

        private System.Threading.Tasks.Task SendVersion()
        {
            var message = new DiscordMessage
            {
                embeds = new[]
                {
                new DiscordEmbed
                {
                    title = "New version compiled",
                    description = $"{Application.productName} - v{Application.version}",
                    color = 5814783
                }
            }
            };

            return SendDiscordWebhook(message);
        }

        private async System.Threading.Tasks.Task SendDiscordWebhook(DiscordMessage message)
        {
            string json = JsonUtility.ToJson(message);
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(json);

            using UnityWebRequest www = new UnityWebRequest(WEBHOOK_URL, "POST")
            {
                uploadHandler = new UploadHandlerRaw(payload),
                downloadHandler = new DownloadHandlerBuffer()
            };

            www.SetRequestHeader("Content-Type", "application/json");

            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await System.Threading.Tasks.Task.Delay(100);

            if (www.result != UnityWebRequest.Result.Success)
                UnityEngine.Debug.LogError($"Error Discord: {www.error}");
            else
                UnityEngine.Debug.Log("Notification sent to Discord");
        }

        [System.Serializable]
        private class DiscordMessage
        {
            public string username = BOT_NAME;
            public DiscordEmbed[] embeds;
        }

        [System.Serializable]
        private class DiscordEmbed
        {
            public string title;
            public string description;
            public int color;
        }
    }
}

