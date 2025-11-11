using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public sealed class Pets : IData
    {
        public string desc;
        public string title;
        public string image;
        public string skillName;
        public string flavorText;
        public string type;

        public static Pets Parse(JObject j) =>
            Map(j, new Dictionary<string, Action<Pets, JToken>>
            {
                { "desc",       (o,v) => o.desc       = AsString(v) },
                { "title",      (o,v) => o.title      = AsString(v) },
                { "image",      (o,v) => o.image      = AsString(v) },
                { "skillName",  (o,v) => o.skillName  = AsString(v) },
                { "flavorText", (o,v) => o.flavorText = AsString(v) },
                { "type",       (o,v) => o.type       = AsString(v) },
            });
    }
}