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
                { "content",  (o,v) => o.content = AsString(v) },
                { "title",    (o,v) => o.title   = AsString(v) },
                { "category", (o,v) => o.category= AsString(v) },
            });
    }
}