using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Data
{
    public static class ContentParsing
    {
        public static T Map<T>(JObject j, Dictionary<string, Action<T, JToken>> map) where T : new()
        {
            var t = new T();
            foreach (var kv in map)
            {
                var tok = j.SelectToken(kv.Key);
                if (tok == null) continue;
                kv.Value(t, tok);
            }
            return t;
        }

        public static List<T> MapArray<T>(JToken token, Func<JObject, T> parseOne)
        {
            var list = new List<T>();
            if (token is not JArray arr) return list;
            foreach (var el in arr)
                if (el is JObject o) list.Add(parseOne(o));
            return list;
        }

        public static string AsString(JToken t, string def = "") =>
            t == null || t.Type == JTokenType.Null ? def : t.ToString();

        public static bool AsBool(JToken t, bool def = false)
        {
            if (t == null || t.Type == JTokenType.Null) return def;
            if (t.Type == JTokenType.Boolean) return t.Value<bool>();
            var s = t.ToString().Trim();
            if (s == "1") return true; if (s == "0") return false;
            return bool.TryParse(s, out var b) ? b : def;
        }

        public static int AsInt(JToken t, int def = 0)
        {
            if (t == null || t.Type == JTokenType.Null) return def;
            if (t.Type == JTokenType.Integer) return t.Value<int>();
            return int.TryParse(t.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : def;
        }

        public static DateTime AsDate(JToken t, DateTime def = default)
        {
            if (t == null || t.Type == JTokenType.Null) return def;
            // ISO-8601 / RFC-ish
            if (t.Type == JTokenType.Date) return t.Value<DateTime>();
            return DateTime.TryParse(t.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var d)
                ? d : def;
        }

        public static string[] AsStringArray(JToken t)
        {
            if (t is not JArray arr) return Array.Empty<string>();
            var res = new string[arr.Count];
            for (int i = 0; i < arr.Count; i++) res[i] = AsString(arr[i]);
            return res;
        }
    }
}
