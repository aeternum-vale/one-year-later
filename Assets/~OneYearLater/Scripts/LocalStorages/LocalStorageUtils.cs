using UnityEngine;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

namespace OneYearLater.LocalStorages
{
	public static class LocalStorageUtils
	{
		public static string GetDbPathOnDevice(string dbNameWithExtension)
		{

#if UNITY_EDITOR
			var dbPath = string.Format(@"Assets/StreamingAssets/{0}", dbNameWithExtension);
#else
			// check if file exists in Application.persistentDataPath
			var filepath = string.Format("{0}/{1}", Application.persistentDataPath, dbNameWithExtension);

			if (!File.Exists(filepath))
			{
				// if it doesn't ->
				// open StreamingAssets directory and load the db ->
#if UNITY_ANDROID
				var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + dbNameWithExtension);  // this is the path to your StreamingAssets in android
				while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
				// then save to Application.persistentDataPath
				File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
				var loadDb = Application.dataPath + "/Raw/" + dbNameWithExtension;  // this is the path to your StreamingAssets in iOS
				// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);
#elif UNITY_WP8
				var loadDb = Application.dataPath + "/StreamingAssets/" + dbNameWithExtension;  // this is the path to your StreamingAssets in iOS
				// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);

#elif UNITY_WINRT
				var loadDb = Application.dataPath + "/StreamingAssets/" + dbNameWithExtension;  // this is the path to your StreamingAssets in iOS
				// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);
			
#elif UNITY_STANDALONE_OSX
				var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + dbNameWithExtension;  // this is the path to your StreamingAssets in iOS
				// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);
#else
				var loadDb = Application.dataPath + "/StreamingAssets/" + dbNameWithExtension;  // this is the path to your StreamingAssets in iOS
																								// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);

#endif
			}

			var dbPath = filepath;
#endif
			return dbPath;
		}
	}
}