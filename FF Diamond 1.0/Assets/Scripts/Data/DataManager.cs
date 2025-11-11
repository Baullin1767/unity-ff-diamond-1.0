using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Plugins.Dropbox;

namespace Data
{
    public static class DataManager
    {
        private static readonly Dictionary<DataType, Func<string, IData[]>> Parsers = new()
        {
            [DataType.RedeemCodes] = j => ContentFeedParserManual.ParseArray(j, "", RedeemCodes.Parse).ToArray(),
            [DataType.TipsATricks] = j => ContentFeedParserManual.ParseArray(j, "", TipsATricks.Parse).ToArray(),
            [DataType.FreeDiamond] = j => ContentFeedParserManual.ParseArray(j, "", FreeDiamond.Parse).ToArray(),
            [DataType.GameVehicles] = j => ContentFeedParserManual.ParseArray(j, "", GameVehicles.Parse).ToArray(),
            [DataType.GameWeapons] = j => ContentFeedParserManual.ParseArray(j, "", GameWeapons.Parse).ToArray(),
            [DataType.Pets] = j => ContentFeedParserManual.ParseArray(j, "", Pets.Parse).ToArray(),
            [DataType.Quiz] = j => ContentFeedParserManual.ParseArray(j, "", Quiz.Parse).ToArray(),
        };

        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
        private static readonly Dictionary<DataType, (string json, DateTime fetchedUtc)> Cache = new();

        public static async UniTask<IData[]> GetItemData(DataType dataType, CancellationToken ct = default)
        {
            if (!Parsers.TryGetValue(dataType, out var parse))
                throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);

            var json = await GetJsonCached(dataType, ct);
            return parse(json);
        }

        private static async UniTask<string> GetJsonCached(DataType type, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            if (Cache.TryGetValue(type, out var entry) && (now - entry.fetchedUtc) < CacheTtl)
                return entry.json;

            var path = PathBuilder.GetJsonPath(type);
            var json = await DropboxHelper.DownloadText(path).AttachExternalCancellation(ct);

            Cache[type] = (json, now);
            return json;
        }
    }
}