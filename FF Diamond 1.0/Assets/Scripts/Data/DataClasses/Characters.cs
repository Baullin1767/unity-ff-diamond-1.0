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
        public bool IsPaid;

        public sealed class Biography : IData
        {
            public string gender;
            public int age;
            public string birthday;
            public string story;

            public static Biography Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Biography, JToken>>
                {
                    { "24gdfg",   (o,v) => o.gender   = AsString(v) },
                    { "234fdf",      (o,v) => o.age      = AsInt(v) },
                    { "765hhgfh", (o,v) => o.birthday = AsString(v) },
                    { "45khgh",    (o,v) => o.story    = AsString(v) },
                });
        }

        public struct Skill
        {
            public string skillName;
            public string skillDesc;

            public static Skill Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Skill, JToken>>
                {
                    { "5hgfh", (o,v) => o.skillName = AsString(v) },
                    { "5ghfdh", (o,v) => o.skillDesc = AsString(v) },
                });
        }

        public static Characters Parse(JObject j)
        {
            var character = Map(j, new Dictionary<string, Action<Characters, JToken>>
            {
                { "hjjkhjmh",  (o,v) => o.tagline = AsString(v) },
                { "67mgnm",     (o,v) => o.name    = AsString(v) },
                { "6gjghgfg",    (o,v) => o.image   = AsString(v) },
                { "67jghfhdg",    (o,v) => o.skill   = Skill.Parse((JObject)v) },
                { "5hfghh6",(o,v) => o.biography = Biography.Parse((JObject)v) },
                { "54ggfdhh",(o,v) => o.IsPaid = !(AsInt(v) == 1 || AsInt(v) == 0)},
            });

            character.IsPaid = PurchaseStateStorage.GetCharacterIsPaid(character, character.IsPaid);
            return character;
        }
    }
}
