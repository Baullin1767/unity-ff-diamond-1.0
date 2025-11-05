using System.Collections.Generic;
using UnityEngine;

namespace Plugins.Dropbox
{
	public static class PathBuilder
	{
		private static readonly Dictionary<DataType, string> _directoryName = new()
		{
			{ DataType.CustomContent, "56jhrfrhrh/FJIRIOI" },
			{ DataType.Packs, "56jhrfrhrh/FDSFFD" },
			{ DataType.Castomizer, "56jhrfrhrh/SFEEEFS" },
			{ DataType.SecretCodes, "56jhrfrhrh/FDVSVS" },
			{ DataType.Furnitures, "56jhrfrhrh/GDFGGG" },
			{ DataType.HouseBuilder, "56jhrfrhrh/45JIKFEO" },
		};

		private static readonly Dictionary<DataType, string> _jsonPaths = new()
		{
			{ DataType.CustomContent, "56jhrfrhrh/FJIRIOI/content.json" },
			{ DataType.Packs, "56jhrfrhrh/FDSFFD/content.json" },
			{ DataType.Castomizer, "56jhrfrhrh/SFEEEFS/content.json" },
			{ DataType.SecretCodes, "56jhrfrhrh/FDVSVS/content.json" },
			{ DataType.Furnitures, "56jhrfrhrh/GDFGGG/content.json" },
			{ DataType.HouseBuilder, "56jhrfrhrh/45JIKFEO/content.json" },
		};

		public static string GetRemoteJsonPath(DataType type) => _jsonPaths[type];

		public static string GetRemoteFilePath(DataType type, string fileName) =>
			$"{_directoryName[type]}/{fileName}";

		public static string GetLocalJsonPath(DataType type) =>
			Application.persistentDataPath + $"Resources/{type}.json";

		public static string GetLocalFilePath(DataType type, string fileName) =>
			$"{Application.persistentDataPath}/{_directoryName[type]}/{fileName}";
	}

	public enum DataType
	{
		CustomContent = 0,
		Packs = 1,
		Castomizer = 2,
		SecretCodes = 3,
		Furnitures = 4,
		HouseBuilder = 5
	}
}