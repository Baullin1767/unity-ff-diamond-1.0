using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Plugins.Dropbox;
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
                ParseArray(j, "54hhbf43g", "", Quiz.Parse).ToArray(),
            [DataType.Characters] = j => 
                ParseArray(j, "7rytryty", "Characters", Characters.Parse).ToArray(),
        };

        public static async UniTask<IData[]> GetItemData(DataType dataType, CancellationToken ct = default)
        {
            if (!Parsers.TryGetValue(dataType, out var parse))
                throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);

            var json = await GetJsonCached(dataType, ct);
            return parse(json);
        }

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
    }
}