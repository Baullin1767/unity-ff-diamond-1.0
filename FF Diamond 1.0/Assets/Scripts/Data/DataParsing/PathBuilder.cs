using System;
using System.IO;

namespace Data
{
    public class PathBuilder
    {
        private static string baseFolder = "45tgrfgsfdv";
        private static string baseFileName = "content.json";
        public static string GetJsonPath(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.RedeemCodes:
                    return Path.Combine(baseFolder, "FDIDDS", baseFileName);
                case DataType.TipsATricks:
                    return Path.Combine(baseFolder, "CAASF", baseFileName);
                case DataType.FreeDiamond:
                    return Path.Combine(baseFolder, "FDSVSA", baseFileName);
                case DataType.GameVehicles:
                    return Path.Combine(baseFolder, "FSAECS", baseFileName);
                case DataType.GameWeapons:
                    return Path.Combine(baseFolder, "CSAQDF", baseFileName);
                case DataType.Pets:
                    return Path.Combine(baseFolder, "VSDSFE", baseFileName);
                case DataType.Quiz:
                    return Path.Combine(baseFolder, "34gdfS", baseFileName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
    }
}