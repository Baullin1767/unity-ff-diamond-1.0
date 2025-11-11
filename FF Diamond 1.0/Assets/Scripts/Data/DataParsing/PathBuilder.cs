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
                    return $"{baseFolder}/FDIDDS/{baseFileName}";
                case DataType.TipsATricks:
                    return $"{baseFolder}/CAASF/{baseFileName}";
                case DataType.FreeDiamond:
                    return $"{baseFolder}/FDSVSA/{baseFileName}";
                case DataType.GameVehicles:
                    return $"{baseFolder}/FSAECS/{baseFileName}";
                case DataType.GameWeapons:
                    return $"{baseFolder}/CSAQDF/{baseFileName}";
                case DataType.Pets:
                    return $"{baseFolder}/VSDSFE/{baseFileName}";
                case DataType.Quiz:
                    return $"{baseFolder}/34gdfS/{baseFileName}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
    }
}