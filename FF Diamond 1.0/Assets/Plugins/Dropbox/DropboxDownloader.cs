using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Plugins.Dropbox.Exceptions;
using UnityEngine;
using UnityEngine.Networking;

namespace Plugins.Dropbox
{
    public static partial class DropboxHelper
    {
        public static async Task<byte[]> DownloadAsset(string path,
            CancellationToken cancellationToken = default, IProgress<float> progress = null)
        {
            using var downloadHandler = await GetDownloadResult(path, cancellationToken, progress);
            var result = downloadHandler.data;
            return result;
        }

        public static async Task<string> DownloadText(string path,
            CancellationToken cancellationToken = default, IProgress<float> progress = null)
        {
            using var downloadHandler = await GetDownloadResult(path, cancellationToken, progress);
            if(downloadHandler != null)
                return downloadHandler.text;
            return null;
        }

        private static async Task<DownloadHandler> GetDownloadResult(string path,
             CancellationToken cancellationToken = default, IProgress<float> progress = null)
        {
            UnityWebRequest request;
            UnityWebRequest asyncRequest;

            try
            {
                request = GetRequestForFileDownload(path);
                asyncRequest = await request.SendWebRequest()
                    .ToUniTask(progress: progress, cancellationToken: cancellationToken)
                    .Timeout(TimeSpan.FromSeconds(20));
            }
            catch (Exception ex) when (ex is UnityWebRequestException || ex is TimeoutException)
            {
                return null;
            }

            if (request.responseCode != 200)
            {
                throw new WrongRequestException(path);
            }

            return asyncRequest.downloadHandler;
        }


        public static async Task<Texture2D> DownloadTexture(string path,
    CancellationToken cancellationToken = default, IProgress<float> progress = null)
        {

            DownloadHandler result = null;
            int maxRetries = 3;
            int delayBetweenRetries = 2000;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    result = await GetDownloadResult(path, cancellationToken, progress);
                    break;
                }
                catch (Exception ex) when (ex is UnityWebRequestException || ex is TimeoutException)
                {
                    if (attempt == maxRetries - 1)
                    {
                        throw;
                    }
                    await Task.Delay(delayBetweenRetries, cancellationToken);
                }
            }

            if (result == null)
            {
                return null;
            }

            var content = result.data;



            var tex = new Texture2D(2, 2);
            tex.LoadImage(content);
            return tex;
        }
    }
}