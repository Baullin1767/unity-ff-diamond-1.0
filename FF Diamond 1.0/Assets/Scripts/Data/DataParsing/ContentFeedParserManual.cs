using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public static class ContentFeedParserManual
    {
        public static List<T> ParseArray<T>(string json, string jsonArrayPath, Func<JObject, T> parseOne)
        {
            var root = JToken.Parse(json);
            var token = string.IsNullOrWhiteSpace(jsonArrayPath)
                ? root
                : root.SelectToken(jsonArrayPath);
            return MapArray(token, parseOne);
        }
    }
}