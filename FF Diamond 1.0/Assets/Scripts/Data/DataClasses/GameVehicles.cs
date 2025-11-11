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
                    { "health",   (o,v) => o.health   = AsInt(v) },
                    { "speed",    (o,v) => o.speed    = AsInt(v) },
                    { "capacity", (o,v) => o.capacity = AsInt(v) },
                });
        }

        public static GameVehicles Parse(JObject j) =>
            Map(j, new Dictionary<string, Action<GameVehicles, JToken>>
            {
                { "desc",   (o,v) => o.desc  = AsString(v) },
                { "title",  (o,v) => o.title = AsString(v) },
                { "image",  (o,v) => o.image = AsString(v) },
                // вложенность: "skills" -> объект
                { "skills", (o,v) => { if (v is JObject o2) o.skills = Skills.Parse(o2); } },
            });
    }
}