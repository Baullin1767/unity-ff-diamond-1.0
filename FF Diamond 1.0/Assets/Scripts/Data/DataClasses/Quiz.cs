using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public sealed class Quiz : IData
    {
        public Question[] Questions;
        public Results[] ResultsList;

        public sealed class Question : IData
        {
            public string question;
            public Answer[] aVariantOfTheAnswers;

            public sealed class Answer : IData
            {
                public string variant;
                public bool isRight;

                public static Answer Parse(JObject j) =>
                    Map(j, new Dictionary<string, Action<Answer, JToken>>
                    {
                        { "variant", (o,v) => o.variant = AsString(v) },
                        { "isRight", (o,v) => o.isRight = AsBool(v)  },
                    });
            }

            public static Question Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Question, JToken>>
                {
                    { "question", (o,v) => o.question = AsString(v) },
                    { "aVariantOfTheAnswers", (o,v) =>
                        o.aVariantOfTheAnswers = MapArray(v, o2 => Answer.Parse(o2)).ToArray() },
                });
        }

        public sealed class Results : IData
        {
            public string name;
            public string desc;
            public string image;

            public static Results Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Results, JToken>>
                {
                    { "name",  (o,v) => o.name  = AsString(v) },
                    { "desc",  (o,v) => o.desc  = AsString(v) },
                    { "image", (o,v) => o.image = AsString(v) },
                });
        }

        public static Quiz Parse(JObject j)
        {
            var q = new Quiz();
            q.Questions = MapArray(j.SelectToken("Questions"), o => Question.Parse(o)).ToArray();
            q.ResultsList   = MapArray(j.SelectToken("Results"),   o => Results.Parse(o)).ToArray();
            return q;
        }
    }
}
