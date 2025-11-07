namespace Data.GiftCodes
{
    public struct GiftCodesData : IData
    {
        public string code;

        public GiftCodesData(string code)
        {
            this.code = code;
        }
    }
}