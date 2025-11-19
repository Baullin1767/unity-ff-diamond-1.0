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
        public bool isPaid;

        public static RedeemCodes Parse(JObject j)
        {
            var redeemCode = Map(new JObjectSafe(j), new Dictionary<string, Action<RedeemCodes, JToken>>
            {
                { "23fdsf", (o, v) => o.desc = AsString(v) },
                { "gfjkgfd", (o, v) => o.code = AsString(v) },
                { "45gfdhfh", (o, v) => o.date = AsDate(v) },
                { "43gfgdf", (o, v) => o.isActive = AsBool(v) },
                { "56hfdgfsdf",(o,v) => o.isPaid = !(AsInt(v) == 1 || AsInt(v) == 0)},
            });

            redeemCode.isPaid = PurchaseStateStorage.GetRedeemCodeIsPaid(redeemCode, redeemCode.isPaid);
            return redeemCode;
        }

        private sealed class JObjectSafe : JObject
        {
            public JObjectSafe(JObject j) : base(j)
            {
            }
        }
    }
}
