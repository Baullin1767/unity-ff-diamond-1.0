using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Plugins.Dropbox
{
    public static partial class DropboxHelper
    {
        private const string AppKey = "hc29j1jdd2onvf9";
        private const string AppSecret = "w62ypmwfybu3a5l";

#if UNITY_EDITOR
		private const string AuthCode = "k_NEn1_GHiMAAAAAAAAwWQREpwsyx1oMMFLrcC8gF_g";
#endif
        private const string RefreshToken = "PdOeX1s0D1IAAAAAAAAAAc0lmbKlY6iZrRHLG2rmsmoYqaT1PnXZ8wu7RF4gvAEw";

		private static string _tempRuntimeToken = null;

#if UNITY_EDITOR
        // First, call this method to get an authCode, then paste it in the appropriate field above.
        public static void GetAuthCode()
        {
            var url = $"https://www.dropbox.com/oauth2/authorize?client_id={AppKey}&response_type=code&token_access_type=offline";
            Application.OpenURL(url);
        }

        // After you have pasted an AuthCode, call this method to get refreshToken.
        public static async void GetRefreshToken()
        {
            Debug.Log("Requesting refreshToken...");

            var form = new WWWForm();
            form.AddField("code", AuthCode);
            form.AddField("grant_type", "authorization_code");

            var base64Authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{AppKey}:{AppSecret}"));

            using var request = UnityWebRequest.Post("https://api.dropbox.com/oauth2/token", form);
            request.SetRequestHeader("Authorization", $"Basic {base64Authorization}");

            var sendRequest = request.SendWebRequest();

            while (!sendRequest.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
                return;
            }

            var parsedAnswer = JObject.Parse(request.downloadHandler.text);
            var refreshTokenString = parsedAnswer["refresh_token"]?.Value<string>();

            Debug.Log("Copy this string to RefreshToken: " + refreshTokenString);
        }
#endif

        /// <summary>
        /// Call initialization before you start download any files and await it's completion.
        /// To wait inside a coroutine you can use:
        /// var task = DropboxHelper.Initialize();
        /// yield return new WaitUntil(() => task.IsCompleted);
        /// </summary>
        private static bool callForUInitial;

        public static async Task Initialize()
        {
            if (callForUInitial)
                return;

            callForUInitial = true;
            if (string.IsNullOrEmpty(RefreshToken))
            {
                Debug.LogError("refreshToken should be defined before runtime");
            }

            var form = new WWWForm();
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", RefreshToken);

            var base64Authorization =
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{AppKey}:{AppSecret}"));

            using var request = UnityWebRequest.Post("https://api.dropbox.com/oauth2/token", form);
            request.SetRequestHeader("Authorization", $"Basic {base64Authorization}");

            var sendRequest = request.SendWebRequest();

            while (!sendRequest.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                IsInitialized = true;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
            }

            // Debug.Log("Success! Full message from dropbox: " + request.downloadHandler.text);

            var data = JObject.Parse(request.downloadHandler.text);
            _tempRuntimeToken = data["access_token"]?.Value<string>();

            callForUInitial = false;
        }

        public static bool IsInitialized = false;

        public static UnityWebRequest GetRequestForRootFolder()
        {
            var request = UnityWebRequest.Get("https://content.dropboxapi.com/2/files/download");
            request.SetRequestHeader("Authorization", $"Bearer {_tempRuntimeToken}");
            request.SetRequestHeader("Dropbox-API-Arg", $"{{\"path\": \"/{string.Empty}\"}}");
            return request;
        }

        /// <summary>
        /// Creating a request for file download.
        /// To wait inside a coroutine you can use:
        /// var task = DropboxHelper.GetRequestForFileDownload();
        /// yield return new WaitUntil(() => task.IsCompleted);
        /// </summary>
        /// <param name="relativePathToFile">Pass a relative path from a root folder. Example: "images/image1"</param>
        /// <returns>WebRequest that you should send and then process it's result</returns>
        public static UnityWebRequest GetRequestForFileDownload(string relativePathToFile)
        {
            var request = UnityWebRequest.Get("https://content.dropboxapi.com/2/files/download");
            request.SetRequestHeader("Authorization", $"Bearer {_tempRuntimeToken}");
            request.SetRequestHeader("Dropbox-API-Arg", $"{{\"path\": \"/{relativePathToFile}\"}}");
            return request;
        }

        public static UnityWebRequest GetRequestForAudioClipDownload(string relativePathToFile,
            AudioType audioType)
        {
            var request = new UnityWebRequest("https://content.dropboxapi.com/2/files/download", "GET",
                new DownloadHandlerAudioClip("https://content.dropboxapi.com/2/files/download",
                    audioType), null);
            request.SetHeaders(relativePathToFile);
            return request;
        }

        public static UnityWebRequest GetRequestForFileMetadata(string relativePathToFile)
        {
            var url = "https://api.dropboxapi.com/2/files/get_metadata";
            var jsonBody = "{\"path\":\"/" + relativePathToFile + "\"}";
            var request = new UnityWebRequest(url, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", $"Bearer {_tempRuntimeToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log(request);

            return request;
        }

        private static void SetHeaders(this UnityWebRequest request, string relativePathToFile)
        {
            if (_tempRuntimeToken == null)
            {
                Debug.LogError("DropboxHelper must be initialized before constructing requests");
                return;
            }

            request.SetRequestHeader("Authorization", $"Bearer {_tempRuntimeToken}");
            request.SetRequestHeader("Dropbox-API-Arg", $"{{\"path\": \"/{relativePathToFile}\"}}");
        }

        public static UnityWebRequest GetRequestForListFolder(string relativePathToFile)
        {
            var url = "https://api.dropboxapi.com/2/files/list_folder";

            var jsonBody = "{\"path\":\"" + relativePathToFile + "\"}";
            var request = new UnityWebRequest(url, "POST");

            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", $"Bearer {_tempRuntimeToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            return request;
        }
    }
}