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
            public Answer[] answers;

            public sealed class Answer : IData
            {
                public string aVariantOfTheAnswers;
                public bool answer;

                public static Answer Parse(JObject j) =>
                    Map(j, new Dictionary<string, Action<Answer, JToken>>
                    {
                        { "4hlgdfg", (o,v) => o.aVariantOfTheAnswers = AsString(v) },
                        { "2ddf", (o,v) => o.answer = AsBool(v)  },
                    });
            }

            public static Question Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Question, JToken>>
                {
                    { "45gf423", (o,v) => o.question = AsString(v) },
                    { "354gdf5", (o,v) =>
                        o.answers = MapArray(v, o2 => Answer.Parse(o2)).ToArray() },
                });
        }

        public sealed class Results : IData
        {
            public string score;
            public string name;
            public string desc;
            public string image;

            public static Results Parse(JObject j) =>
                Map(j, new Dictionary<string, Action<Results, JToken>>
                {
                    { "gjdfk",  (o,v) => o.score  = AsString(v) },
                    { "68hjhg",  (o,v) => o.name  = AsString(v) },
                    { "57jghj",  (o,v) => o.desc  = AsString(v) },
                    { "6hfghf", (o,v) => o.image = AsString(v) },
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
