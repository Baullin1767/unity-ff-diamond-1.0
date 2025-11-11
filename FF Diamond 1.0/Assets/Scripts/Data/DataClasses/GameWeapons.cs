using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public sealed class GameWeapons : IData
    {
        public string desc;
        public string title;
        public string image;
        public Stats stats;
        public string[] tags;

        public sealed class Stats
        {
            public int damage;
            public int rateOfFire;
            public int range;
            public string ammoType;

            public static Stats Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Stats, JToken>>
                {
                    { "damage",     (o,v) => o.damage    = AsInt(v)    },
                    { "rateOfFire", (o,v) => o.rateOfFire= AsInt(v)    },
                    { "range",      (o,v) => o.range     = AsInt(v)    },
                    { "ammoType",   (o,v) => o.ammoType  = AsString(v) },
                });
        }

        public static GameWeapons Parse(JObject j) =>
            Map(j, new Dictionary<string, Action<GameWeapons, JToken>>
            {
                { "desc",  (o,v) => o.desc  = AsString(v) },
                { "title", (o,v) => o.title = AsString(v) },
                { "image", (o,v) => o.image = AsString(v) },
                { "stats", (o,v) => { if (v is JObject o2) o.stats = Stats.Parse(o2); } },
                { "tags",  (o,v) => o.tags = AsStringArray(v) },
            });
    }
}