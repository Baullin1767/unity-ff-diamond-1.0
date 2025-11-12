using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public sealed class FreeDiamond : IData
    {
        public string content;
        public string title;
        public string difficulty;

        public static FreeDiamond Parse(JObject j) =>
            Map(j, new Dictionary<string, Action<FreeDiamond, JToken>>
            {
                { "23dasfsd",   (o,v) => o.content    = AsString(v) },
                { "67hgjghj",     (o,v) => o.title      = AsString(v) },
                { "34gfdgfg",(o,v) => o.difficulty = AsString(v) },
            });
    }
}