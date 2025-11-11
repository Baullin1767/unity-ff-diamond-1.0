using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public sealed class Characters : IData
    {
        public string tagline;
        public string name;
        public string image;
        public Skill skill;
        public Biography biography;

        public sealed class Biography : IData
        {
            public string gender;
            public int age;
            public string birthday;
            public string story;

            public static Biography Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Biography, JToken>>
                {
                    { "gender",   (o,v) => o.gender   = AsString(v) },
                    { "age",      (o,v) => o.age      = AsInt(v) },
                    { "birthday", (o,v) => o.birthday = AsString(v) },
                    { "story",    (o,v) => o.story    = AsString(v) },
                });
        }

        public struct Skill
        {
            public string skillName;
            public string skillDesc;

            public static Skill Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Skill, JToken>>
                {
                    { "skillName", (o,v) => o.skillName = AsString(v) },
                    { "skillDesc", (o,v) => o.skillDesc = AsString(v) },
                });
        }

        public static Characters Parse(JObject j) =>
            Map(j, new Dictionary<string, Action<Characters, JToken>>
            {
                { "tagline",  (o,v) => o.tagline = AsString(v) },
                { "name",     (o,v) => o.name    = AsString(v) },
                { "image",    (o,v) => o.image   = AsString(v) },
                { "skill",    (o,v) => o.skill   = Skill.Parse((JObject)v) },
                { "biography",(o,v) => o.biography = Biography.Parse((JObject)v) },
            });
    }
}
