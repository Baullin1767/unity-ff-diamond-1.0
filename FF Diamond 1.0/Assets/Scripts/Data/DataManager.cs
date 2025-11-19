using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Plugins.Dropbox;
using UnityEngine;
using static Data.ContentFeedParserManual;

namespace Data
{
    public static class DataManager
    {
        private static readonly Dictionary<DataType, Func<string, IData[]>> Parsers = new()
        {
            [DataType.RedeemCodes] = j => 
                ParseArray(j, "43jjyjt", "Redeem Codes", RedeemCodes.Parse).ToArray(),
            [DataType.TipsATricks] = j => 
                ParseArray(j, "56hfgfgnvvv", "Tips and Tricks", TipsATricks.Parse).ToArray(),
            [DataType.FreeDiamond] = j => 
                ParseArray(j, "7hgfhfgh4", "Free Diamond", FreeDiamond.Parse).ToArray(),
            [DataType.GameVehicles] = j => 
                ParseArray(j, "54hfdgdg6", "Game Vehicles", GameVehicles.Parse).ToArray(),
            [DataType.GameWeapons] = j => 
                ParseArray(j, "43gdfgdg", "Game Weapons", GameWeapons.Parse).ToArray(),
            [DataType.Pets] = j => 
                ParseArray(j, "45hfgh", "Pets", Pets.Parse).ToArray(),
            [DataType.Quiz] = j =>
            {
                var quiz = ParseObject(j, "54hhbf43g", "", Quiz.Parse);
                return quiz != null ? new IData[] { quiz } : Array.Empty<IData>();
            },
            [DataType.Characters] = j => 
                ParseArray(j, "7rytryty", "Characters", Characters.Parse).ToArray(),
        };

        private const string StorePriceListResourcePath = "StorePriceList";

        private static IData[] _storeItemsCache;
        private static StorePriceList _storePriceList;

        public static async UniTask<IData[]> GetItemsData(DataType dataType, CancellationToken ct = default)
        {
            if (dataType == DataType.Store)
                return GetStoreItemsFromScriptableObject();

            if (!Parsers.TryGetValue(dataType, out var parse))
                throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);

            var json = await GetJsonCached(dataType, ct);
            return parse(json);
        }

        private static readonly string SpriteCacheRoot =
            Path.Combine(Application.temporaryCachePath, "DropboxSpriteCache");

        private const float SpritePixelsPerUnit = 100f;

        private static async UniTask<string> GetJsonCached(DataType type, CancellationToken ct)
        {
            var cachedJson = DropboxJsonCache.GetJson(type);
            if (!string.IsNullOrEmpty(cachedJson))
                return cachedJson;

            var path = PathBuilder.GetJsonPath(type);
            var json = await DropboxHelper.DownloadText(path).AttachExternalCancellation(ct);

            await DropboxJsonCache.StoreAsync(type, json);
            return json;
        }

        public static async UniTask<Sprite> GetSprite(string relativePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            var cached = await TryGetSpriteFromCache(relativePath);
            if (cached != null)
                return cached;

            var downloaded = await DropboxHelper.DownloadSprite(relativePath, ct);
            if (downloaded == null)
                return null;

            await StoreSpriteAsync(relativePath, downloaded.texture);
            return downloaded;
        }

        private static async UniTask<Sprite> TryGetSpriteFromCache(string relativePath)
        {
            var path = BuildSpriteCachePath(relativePath);
            if (!File.Exists(path))
                return null;

            byte[] bytes;
            try
            {
                bytes = await File.ReadAllBytesAsync(path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to read cached sprite at {path}: {ex.Message}");
                return null;
            }

            return CreateSpriteFromBytes(bytes);
        }

        private static async UniTask StoreSpriteAsync(string relativePath, Texture2D texture)
        {
            if (texture == null)
                return;

            var bytes = texture.EncodeToPNG();
            var path = BuildSpriteCachePath(relativePath);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            try
            {
                await File.WriteAllBytesAsync(path, bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to cache sprite at {path}: {ex.Message}");
            }
        }

        private static Sprite CreateSpriteFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            var texture = new Texture2D(2, 2);
            if (!texture.LoadImage(bytes))
                return null;

            return Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                SpritePixelsPerUnit);
        }

        private static string BuildSpriteCachePath(string relativePath)
        {
            var sanitized = relativePath.Replace('\\', Path.DirectorySeparatorChar);
            sanitized = sanitized.Replace('/', Path.DirectorySeparatorChar);
            sanitized = sanitized.TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(SpriteCacheRoot, sanitized);
        }

        private static IData[] GetStoreItemsFromScriptableObject()
        {
            if (_storeItemsCache != null)
                return _storeItemsCache;

            var asset = LoadStorePriceList();
            if (asset == null)
            {
                _storeItemsCache = Array.Empty<IData>();
                return _storeItemsCache;
            }

            var prices = asset.Prices;
            if (prices == null || prices.Count == 0)
            {
                _storeItemsCache = Array.Empty<IData>();
                return _storeItemsCache;
            }

            var items = new Store[prices.Count];
            for (int i = 0; i < prices.Count; i++)
            {
                var price = Mathf.Max(0, prices[i]);
                items[i] = new Store { price = price };
            }

            _storeItemsCache = items;
            return _storeItemsCache;
        }

        private static StorePriceList LoadStorePriceList()
        {
            if (_storePriceList != null)
                return _storePriceList;

            if (!string.IsNullOrWhiteSpace(StorePriceListResourcePath))
                _storePriceList = Resources.Load<StorePriceList>(StorePriceListResourcePath);

            if (_storePriceList == null)
            {
                var allAssets = Resources.LoadAll<StorePriceList>(string.Empty);
                if (allAssets != null && allAssets.Length > 0)
                    _storePriceList = allAssets[0];
            }

            if (_storePriceList == null)
                Debug.LogWarning($"Store price list asset not found at Resources/{StorePriceListResourcePath}. Store data will be empty.");

            return _storePriceList;
        }
    }
}
