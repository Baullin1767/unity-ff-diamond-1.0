using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public sealed class GameVehicles : IData
    {
        public string desc;
        public string title;
        public string image;
        public Skills skills;

        public sealed class Skills
        {
            public int health;
            public int speed;
            public int capacity;

            public static Skills Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Skills, JToken>>
                {
                    { "kegk4lgdg",   (o,v) => o.health   = AsInt(v) },
                    { "43gfgdfg",    (o,v) => o.speed    = AsInt(v) },
                    { "54hfdgf32", (o,v) => o.capacity = AsInt(v) },
                });
        }

        public static GameVehicles Parse(JObject j) =>
            Map(j, new Dictionary<string, Action<GameVehicles, JToken>>
            {
                { "45hgfdh65",   (o,v) => o.desc  = AsString(v) },
                { "vdfhtdfyy5",  (o,v) => o.title = AsString(v) },
                { "34gfddhrh",  (o,v) => o.image = AsString(v) },
                { "eggdf5dfg", (o,v) => { if (v is JObject o2) o.skills = Skills.Parse(o2); } },
            });
    }
}