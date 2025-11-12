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
                    return $"{GetBasePath(DataType.RedeemCodes)}/{baseFileName}";
                case DataType.TipsATricks:
                    return $"{GetBasePath(DataType.TipsATricks)}/{baseFileName}";
                case DataType.FreeDiamond:
                    return $"{GetBasePath(DataType.FreeDiamond)}/{baseFileName}";
                case DataType.GameVehicles:
                    return $"{GetBasePath(DataType.GameVehicles)}/{baseFileName}";
                case DataType.GameWeapons:
                    return $"{GetBasePath(DataType.GameWeapons)}/{baseFileName}";
                case DataType.Pets:
                    return $"{GetBasePath(DataType.Pets)}/{baseFileName}";
                case DataType.Quiz:
                    return $"{GetBasePath(DataType.Quiz)}/{baseFileName}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
        public static string GetBasePath(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.RedeemCodes:
                    return $"{baseFolder}/FDIDDS";
                case DataType.TipsATricks:
                    return $"{baseFolder}/CAASF";
                case DataType.FreeDiamond:
                    return $"{baseFolder}/FDSVSA";
                case DataType.GameVehicles:
                    return $"{baseFolder}/FSAECS";
                case DataType.GameWeapons:
                    return $"{baseFolder}/CSAQDF";
                case DataType.Pets:
                    return $"{baseFolder}/VSDSFE";
                case DataType.Quiz:
                    return $"{baseFolder}/34gdfS";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
    }
}