using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static Data.ContentParsing;

namespace Data
{
    public static class ContentFeedParserManual
    {
        public static List<T> ParseArray<T>(string json, string parentProperty, string childProperty, Func<JObject, T> parseOne)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("Attempted to parse empty json.");
                return new List<T>();
            }

            var root = JToken.Parse(json);
            JToken current = root;

            if (!string.IsNullOrWhiteSpace(parentProperty))
            {
                current = (current as JObject)?[parentProperty];
                if (current == null)
                {
                    Debug.LogWarning($"JSON path segment '{parentProperty}' was not found.");
                    return new List<T>();
                }
            }

            if (!string.IsNullOrWhiteSpace(childProperty))
            {
                current = (current as JObject)?[childProperty];
                if (current == null)
                {
                    Debug.LogWarning($"JSON path segment '{childProperty}' was not found.");
                    return new List<T>();
                }
            }

            return MapArray(current, parseOne);
        }
    }
}
