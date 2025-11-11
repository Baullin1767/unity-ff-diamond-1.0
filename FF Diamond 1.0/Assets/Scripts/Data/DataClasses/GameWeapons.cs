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
                    { "sjkfsdmf",     (o,v) => o.damage    = AsInt(v)    },
                    { "wgjkdsg", (o,v) => o.rateOfFire= AsInt(v)    },
                    { "dgnmgfkg",      (o,v) => o.range     = AsInt(v)    },
                    { "agfdvdf",   (o,v) => o.ammoType  = AsString(v) },
                });
        }

        public static GameWeapons Parse(JObject j) =>
            Map(j, new Dictionary<string, Action<GameWeapons, JToken>>
            {
                { "45ggfdgdgf",  (o,v) => o.desc  = AsString(v) },
                { "fbdfvvds", (o,v) => o.title = AsString(v) },
                { "saaxasdasd", (o,v) => o.image = AsString(v) },
                { "hnvekdfv", (o,v) => { if (v is JObject o2) o.stats = Stats.Parse(o2); } },
                { "sdsfeffdg",  (o,v) => o.tags = AsStringArray(v) },
            });
    }
}