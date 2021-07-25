using System.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using OneYearLater.LocalStorageSQLite.Models;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using UnityEngine;
using System.IO;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

namespace OneYearLater.LocalStorageSQLite
{
	class SQLiteLocalStorage : MonoBehaviour, ILocalStorage
	{

		private SQLiteAsyncConnection _connection;
		private SQLiteAsyncConnection _externalConnection;
		[SerializeField] private string _dbNameWithExtension = "db.bytes";
		[SerializeField] private string _mockExternalDbNameWithExtension = "mock_external_db.bytes";
		[SerializeField] private string _backupPostfix = "_backup";



		[SerializeField] private int _type = 1;

		[InfoBox("HH:mm dd-MM-yyyy", EInfoBoxType.Normal)]
		[SerializeField] private string _date = "21:14 24-07-2021";

		[TextArea]
		[SerializeField] private string _content = "Сегодня был фантастически паршивый день!";


		private void Awake()
		{
			string dbPath = GetDbPath(_dbNameWithExtension);
			string externalDbPath = GetDbPath(_mockExternalDbNameWithExtension);

			_connection = new SQLiteAsyncConnection(dbPath);
			_connection.CreateTableAsync<RecordModel>();

			_externalConnection = new SQLiteAsyncConnection(externalDbPath);
		}


		private string GetDbPath(string dbName)
		{

#if UNITY_EDITOR
			var dbPath = string.Format(@"Assets/StreamingAssets/{0}", dbName);
#else
			// check if file exists in Application.persistentDataPath
			var filepath = string.Format("{0}/{1}", Application.persistentDataPath, dbName);

			if (!File.Exists(filepath))
			{
				// if it doesn't ->
				// open StreamingAssets directory and load the db ->
#if UNITY_ANDROID
				var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + dbName);  // this is the path to your StreamingAssets in android
				while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
				// then save to Application.persistentDataPath
				File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
				var loadDb = Application.dataPath + "/Raw/" + dbName;  // this is the path to your StreamingAssets in iOS
				// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);
#elif UNITY_WP8
				var loadDb = Application.dataPath + "/StreamingAssets/" + dbName;  // this is the path to your StreamingAssets in iOS
				// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);

#elif UNITY_WINRT
				var loadDb = Application.dataPath + "/StreamingAssets/" + dbName;  // this is the path to your StreamingAssets in iOS
				// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);
			
#elif UNITY_STANDALONE_OSX
				var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + dbName;  // this is the path to your StreamingAssets in iOS
				// then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);
#else
				var loadDb = Application.dataPath + "/StreamingAssets/" + dbName;  // this is the path to your StreamingAssets in iOS
																				   // then save to Application.persistentDataPath
				File.Copy(loadDb, filepath);

#endif
			}

			var dbPath = filepath;
#endif
			return dbPath;
		}

		public async UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
		{
			DateTime dayStartInc = date.Date;
			DateTime dayEndExc = date.Date.AddDays(1);

			var query = _connection.Table<RecordModel>().Where(r => (r.RecordDateTime >= dayStartInc) && (r.RecordDateTime < dayEndExc));
			return (await query.ToListAsync())
				.Select(rm => new DiaryRecordViewModel(rm.RecordDateTime, rm.Content));
		}

		public UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			return UniTask.CompletedTask;
		}

		[Button]
		public void AddRecord()
		{
			var created = DateTime.Now;
			var record = new RecordModel()
			{
				Type = _type,
				RecordDateTime = DateTime.ParseExact(_date, "HH:mm dd-MM-yyyy", CultureInfo.InvariantCulture),
				Content = _content,
				Hash = GetHashString(
						_type +
						_date.ToString(CultureInfo.InvariantCulture) +
						_content +
						created.ToString(CultureInfo.InvariantCulture)
					),
				IsDeleted = false,
				Created = created,
				AdditionalInfo = "UnityEditor"
			};

			_connection.InsertAsync(record);
		}

		public static byte[] GetHash(string inputString)
		{
			using (HashAlgorithm algorithm = SHA256.Create())
				return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		public static string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
				sb.Append(b.ToString("X2"));

			return sb.ToString();
		}

		[Button]
		private async void SynchronizeLocalAndExternal()
		{
			string originalDbPath = GetDbPath(_dbNameWithExtension);
			string externalDbPath = GetDbPath(_mockExternalDbNameWithExtension);

			//#2
			string dbBackupNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(_dbNameWithExtension)}{_backupPostfix}{ Path.GetExtension(_dbNameWithExtension)}";

			string backupDbPath = GetDbPath(dbBackupNameWithExtension);

			File.Copy(originalDbPath, backupDbPath, true);
			Debug.Log("Backup is created !");

			//#3-4

			AsyncTableQuery<RecordModel> query;

			query = _connection.Table<RecordModel>();
			List<RecordModel> allLocalDbRecords = await query.ToListAsync();
			Dictionary<string, RecordModel> localDbHashDictionary = new Dictionary<string, RecordModel>();
			List<RecordModel> localRecordsToUpdate = new List<RecordModel>();
			List<RecordModel> localRecordsToInsert = new List<RecordModel>();

			query = _externalConnection.Table<RecordModel>();
			List<RecordModel> allExternalDbRecords = await query.ToListAsync();

			allLocalDbRecords.ForEach(r => localDbHashDictionary[r.Hash] = r);

			foreach (var externalRecord in allExternalDbRecords)
			{
				if (localDbHashDictionary.ContainsKey(externalRecord.Hash))
				{
					var localRecord = localDbHashDictionary[externalRecord.Hash];
					if (externalRecord.IsDeleted && !localRecord.IsDeleted)
					{
						localRecord.IsDeleted = true;
						localRecordsToUpdate.Add(localRecord);
					}
					continue;
				}
				localRecordsToInsert.Add(externalRecord);
			}

			var updatedRowsNumber = await _connection.UpdateAllAsync(localRecordsToUpdate);
			Debug.Log($"Local DB, updated rows number: {updatedRowsNumber}");

			var insertedRowsNumber = await _connection.InsertAllAsync(localRecordsToInsert);
			Debug.Log($"Local DB, inserted rows number: {insertedRowsNumber}");


			Debug.Log("Local DB is Updated !");

			//#5

			File.Copy(originalDbPath, externalDbPath, true);

			Debug.Log("Exterbal DB is Replaced by Local !");
		}

	}
}
