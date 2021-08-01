using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using OneYearLater.LocalStorageSQLite.Models;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using TMPro;
using UnityEngine;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

using Debug = UnityEngine.Debug;

namespace OneYearLater.LocalStorageSQLite
{
	class SQLiteLocalStorage : MonoBehaviour, ILocalStorage
	{

		[SerializeField] private DropBoxExternalStorage _dropBox;

		private SQLiteAsyncConnection _connectionToLocal;
		private SQLiteAsyncConnection _connectionToExternalCopy;
		[SerializeField] private string _dbNameWithExtension = "db.bytes";
		[SerializeField] private string _backupPostfix = "_backup";
		[SerializeField] private string _extarnalDbLocalCopyPostfix = "_external";



		[SerializeField] private int _type = 1;

		[InfoBox("HH:mm dd-MM-yyyy", EInfoBoxType.Normal)]
		[SerializeField] private string _date = "21:14 24-07-2021";

		[TextArea]
		[SerializeField] private string _content = "Сегодня был фантастически паршивый день!";

		private bool _rollbackError = false;


		private string GetDbPathOnDevice(string dbNameWithExtension)
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

		public async UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
		{
			DateTime dayStartInc = date.Date;
			DateTime dayEndExc = date.Date.AddDays(1);

			var query = _connectionToLocal.Table<RecordModel>().Where(r => (r.RecordDateTime >= dayStartInc) && (r.RecordDateTime < dayEndExc) && (!r.IsDeleted)).OrderBy(r => r.RecordDateTime);
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
			string originalLocalDbPath = GetDbPathOnDevice(_dbNameWithExtension);
			_connectionToLocal = new SQLiteAsyncConnection(originalLocalDbPath);

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

			_connectionToLocal.InsertAsync(record);

			_connectionToLocal.CloseAsync().Forget();
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

		[SerializeField] private TMP_InputField _accessCodeTextBox;

		public async void Sync()
		{
			Debug.Log($"accessCode is: {_accessCodeTextBox.text}");
			_dropBox.RequestToken(_accessCodeTextBox.text)
				.ContinueWith(SynchronizeLocalAndExternal)
				.Forget();
		}

		[Button]
		private async UniTask<bool> SynchronizeLocalAndExternal()
		{
			string originalLocalDbPath = GetDbPathOnDevice(_dbNameWithExtension);

			string externalDbPath = $"/{_dbNameWithExtension}";

			string externalDbLocalCopyNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(_dbNameWithExtension)}{_extarnalDbLocalCopyPostfix}{ Path.GetExtension(_dbNameWithExtension)}";

			string externalDbLocalCopyPath = GetDbPathOnDevice(externalDbLocalCopyNameWithExtension);

			string dbBackupNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(_dbNameWithExtension)}{_backupPostfix}{ Path.GetExtension(_dbNameWithExtension)}";

			string backupDbPath = GetDbPathOnDevice(dbBackupNameWithExtension);

			// Debug.Log($"originalLocalDbPath: {originalLocalDbPath}");
			// Debug.Log($"externalDbPath: {externalDbPath}");
			// Debug.Log($"externalDbLocalCopyNameWithExtension: {externalDbLocalCopyNameWithExtension}");
			// Debug.Log($"externalDbLocalCopyPath: {externalDbLocalCopyPath}");
			// Debug.Log($"dbBackupNameWithExtension: {dbBackupNameWithExtension}");
			// Debug.Log($"backupDbPath: {backupDbPath}");

			//check
			bool isExternalDbFileExisted = await _dropBox.IsFileExist(externalDbPath);
			Debug.Log($"isExternalDbFileExisted={isExternalDbFileExisted}");

			if (isExternalDbFileExisted)
			{
				try
				{
					File.Copy(originalLocalDbPath, backupDbPath, true);
					Debug.Log("Backup successfully created");
				}
				catch (Exception ex)
				{
					Debug.LogError($"There is an error while creating backup, sync aborted, try later. ({ex.Message}\n{ex.StackTrace})");
					return false;
				}
			}

			try
			{
				if (isExternalDbFileExisted)
				{
					Debug.Log("Start download");
					await _dropBox.DownloadFile(externalDbPath, externalDbLocalCopyPath);
					Debug.Log("End download");

					_connectionToLocal = new SQLiteAsyncConnection(originalLocalDbPath);
					Debug.Log($"_connectionToLocal={_connectionToLocal}");

					AsyncTableQuery<RecordModel> query;

					query = _connectionToLocal.Table<RecordModel>();
					List<RecordModel> allLocalDbRecords = await query.ToListAsync();
					Dictionary<string, RecordModel> localDbHashDictionary = new Dictionary<string, RecordModel>();
					List<RecordModel> localRecordsToUpdate = new List<RecordModel>();
					List<RecordModel> localRecordsToInsert = new List<RecordModel>();

					_connectionToExternalCopy = new SQLiteAsyncConnection(externalDbLocalCopyPath);
					Debug.Log($"_connectionToExternalCopy={_connectionToExternalCopy}");

					query = _connectionToExternalCopy.Table<RecordModel>();
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

					var updatedRowsNumber = await _connectionToLocal.UpdateAllAsync(localRecordsToUpdate);
					var insertedRowsNumber = await _connectionToLocal.InsertAllAsync(localRecordsToInsert);

					Debug.Log("Local DB is Updated !");
				}

				if (_connectionToLocal != null)
					await _connectionToLocal.CloseAsync();
				if (_connectionToExternalCopy != null)
					await _connectionToExternalCopy.CloseAsync();

				await _dropBox.UploadFile(originalLocalDbPath, externalDbPath);

				Debug.Log("Exterbal DB is Replaced by Local !");
				return true;
			}

			catch (Exception ex)
			{
				Debug.LogError($"There is an error while sync, trying to rollback to backup. ({ex.Message}\n{ex.StackTrace})");

				try
				{
					File.Copy(backupDbPath, originalLocalDbPath, true);
					Debug.Log("successful rollback!");
				}

				catch (Exception innerEx)
				{
					Debug.LogError($"There is some error while rolling back to backup. ({innerEx.Message}\n{innerEx.StackTrace})");
					_rollbackError = true;
					return false;
				}
			}

			return false;
		}
	}
}
