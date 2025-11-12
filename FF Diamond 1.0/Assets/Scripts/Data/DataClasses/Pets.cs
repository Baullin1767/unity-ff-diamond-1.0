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
                { "hjjkhjmh",       (o,v) => o.desc       = AsString(v) },
                { "67mgnm",      (o,v) => o.title      = AsString(v) },
                { "6gjghgfg",      (o,v) => o.image      = AsString(v) },
                { "ergdfgdfg",  (o,v) => o.skillName  = AsString(v) },
                { "24vffgdf4", (o,v) => o.flavorText = AsString(v) },
                { "34gfdhn",       (o,v) => o.type       = AsString(v) },
            });
    }
}