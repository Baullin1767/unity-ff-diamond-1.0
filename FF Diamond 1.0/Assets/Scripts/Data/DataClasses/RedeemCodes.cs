using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static Data.ContentParsing;

namespace Data
{
    public sealed class RedeemCodes : IData

    {
        public string desc;
        public string code;
        public DateTime date;
        public bool isActive;

        public static RedeemCodes Parse(JObject j) =>
            Map(new JObjectSafe(j), new Dictionary<string, Action<RedeemCodes, JToken>>
            {
                { "23fdsf", (o, v) => o.desc = AsString(v) },
                { "gfjkgfd", (o, v) => o.code = AsString(v) },
                { "45gfdhfh", (o, v) => o.date = AsDate(v) },
                { "43gfgdf", (o, v) => o.isActive = AsBool(v) },
            });

        private sealed class JObjectSafe : JObject
        {
            public JObjectSafe(JObject j) : base(j)
            {
            }
        }
    }
}