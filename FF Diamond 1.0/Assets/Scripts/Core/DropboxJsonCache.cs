using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Handles storing JSON files downloaded from Dropbox into the application cache and serving them on demand.
    /// </summary>
    public static class DropboxJsonCache
    {
        private static readonly Dictionary<DataType, string> MemoryCache = new();

        private static string CacheRoot =>
            Path.Combine(Application.temporaryCachePath, "DropboxJsonCache");

        private static string BuildFilePath(DataType dataType)
        {
            var relative = PathBuilder.GetJsonPath(dataType);
            return Path.Combine(CacheRoot, relative.Replace('\\', Path.DirectorySeparatorChar));
        }

        public static bool TryGetJson(DataType dataType, out string json)
        {
            if (MemoryCache.TryGetValue(dataType, out json))
                return true;

            var filePath = BuildFilePath(dataType);
            if (File.Exists(filePath))
            {
                json = File.ReadAllText(filePath);
                MemoryCache[dataType] = json;
                return true;
            }

            json = null;
            return false;
        }

        public static string GetJson(DataType dataType)
        {
            return TryGetJson(dataType, out var json) ? json : null;
        }

        public static async UniTask StoreAsync(DataType dataType, string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            MemoryCache[dataType] = json;

            var filePath = BuildFilePath(dataType);
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await UniTask.SwitchToThreadPool();
            File.WriteAllText(filePath, json);
            await UniTask.SwitchToMainThread();
        }
    }
}
