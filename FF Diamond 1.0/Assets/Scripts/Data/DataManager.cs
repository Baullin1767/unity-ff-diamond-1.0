using System;
using System.Collections.Generic;
using Data.GiftCodes;

namespace Data
{
    public class DataManager
    {
        public static IData[] GetItemData(DataType dataType)
        {
            switch(dataType)
            {
                case DataType.GiftCodesData:
                    List<IData> giftCodes = new List<IData>();
                    for (int i = 0; i < 100; i++) giftCodes.Add(new GiftCodesData("Gift Codes Data"));
                    return giftCodes.ToArray();
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }

            return null;
        }
    }
}
