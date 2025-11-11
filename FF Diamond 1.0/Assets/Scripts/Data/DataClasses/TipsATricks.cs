using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public sealed class TipsATricks : IData
    {
        public string content;
        public string title;
        public string category;

        public static TipsATricks Parse(JObject j) =>
            Map(j, new Dictionary<string, Action<TipsATricks, JToken>>
            {
                { "5hfghfgh",  (o,v) => o.content = AsString(v) },
                { "12ddsafsfs",    (o,v) => o.title   = AsString(v) },
                { "64fghfgh", (o,v) => o.category= AsString(v) },
            });
    }
}