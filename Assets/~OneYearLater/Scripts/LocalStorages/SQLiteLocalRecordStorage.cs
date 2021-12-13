using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;

using Debug = UnityEngine.Debug;
using System.Globalization;
using UnityEngine;
using Utilities;
using OneYearLater.Management;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif


namespace OneYearLater.LocalStorages
{
	public class SQLiteLocalRecordStorage : ILocalRecordStorage
	{
		private SQLiteAsyncConnection _connectionToLocal;
		private SQLiteAsyncConnection _connectionToExternalCopy;
		private string _dbNameWithExtension = "records.bytes";
		private string _backupPostfix = "_backup";
		private string _externalDbLocalCopyPostfix = "_external";

		private bool _rollbackError = false;

		public SQLiteLocalRecordStorage()
		{
			string originalLocalDbPath = LocalStorageUtils.GetDbPathOnDevice(_dbNameWithExtension);
			_connectionToLocal = new SQLiteAsyncConnection(originalLocalDbPath);

			_connectionToLocal.CreateTableAsync<SQLiteRecordModel>().Forget();
		}

		public async UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
		{
			DateTime dayStartInc = date.Date;
			DateTime dayEndExc = date.Date.AddDays(1);

			var query =
				_connectionToLocal.Table<SQLiteRecordModel>()
					.Where(r => r.RecordDateTime >= dayStartInc && (r.RecordDateTime < dayEndExc) && (!r.IsDeleted))
					.OrderBy(r => r.RecordDateTime);

			return (await query.ToListAsync())
				.Select(rm => GetDiaryRecordViewModel(rm));
		}

		public async UniTask<BaseRecordViewModel> GetRecordAsync(int recordId)
		{
			var sqliteRecord =
				await _connectionToLocal.Table<SQLiteRecordModel>()
					.Where(r => (r.Id == recordId) && (!r.IsDeleted))
					.FirstAsync();

			return GetDiaryRecordViewModel(sqliteRecord);
		}

		public UniTask InsertRecordAsync(BaseRecordViewModel record)
		{
			switch (record.Type)
			{
				case ERecordKey.Diary:
					return _connectionToLocal.InsertAsync(GetSQLiteRecordModel((DiaryRecordViewModel)record));
				default:
					throw new Exception("invalid record type");
			}
		}

		public UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			List<SQLiteRecordModel> sqliteRecordModels = new List<SQLiteRecordModel>();
			foreach (var record in records)
				switch (record.Type)
				{
					case ERecordKey.Diary:
						sqliteRecordModels.Add(GetSQLiteRecordModel((DiaryRecordViewModel)record));
						break;
				}

			return _connectionToLocal.InsertAllAsync(sqliteRecordModels);
		}

		public UniTask UpdateRecordAsync(BaseRecordViewModel record)
		{
			switch (record.Type)
			{
				case ERecordKey.Diary:
					return _connectionToLocal.UpdateAsync(GetSQLiteRecordModel((DiaryRecordViewModel)record));
				default:
					throw new Exception("invalid record type");
			}
		}

		public async UniTask DeleteRecordAsync(int recordId)
		{
			var sqliteRecord =
				await _connectionToLocal.Table<SQLiteRecordModel>()
					.Where(r => r.Id == recordId)
					.FirstAsync();

			sqliteRecord.IsDeleted = true;

			await _connectionToLocal.UpdateAsync(sqliteRecord);
		}

		public async UniTask<bool> SyncLocalAndExternalRecordStoragesAsync(IExternalStorage externalStorage) //TODO refactor
		{
			string originalLocalDbPath = LocalStorageUtils.GetDbPathOnDevice(_dbNameWithExtension);

			string externalDbPath = $"/{_dbNameWithExtension}";

			string externalDbLocalCopyNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(_dbNameWithExtension)}{_externalDbLocalCopyPostfix}{ Path.GetExtension(_dbNameWithExtension)}";

			string externalDbLocalCopyPath = LocalStorageUtils.GetDbPathOnDevice(externalDbLocalCopyNameWithExtension);

			string dbBackupNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(_dbNameWithExtension)}{_backupPostfix}{ Path.GetExtension(_dbNameWithExtension)}";

			string backupDbPath = LocalStorageUtils.GetDbPathOnDevice(dbBackupNameWithExtension);

			bool isExternalDbFileExisted = await externalStorage.IsFileExist(externalDbPath);
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
					Debug.LogError($"There is an error while creating backup, sync aborted, try later. ({ex.Message})\n{ex.StackTrace})");
					return false;
				}
			}

			try
			{
				if (isExternalDbFileExisted)
				{
					await externalStorage.DownloadFile(externalDbPath, externalDbLocalCopyPath);

					_connectionToLocal = new SQLiteAsyncConnection(originalLocalDbPath);

					var query = _connectionToLocal.Table<SQLiteRecordModel>();
					List<SQLiteRecordModel> allLocalDbRecords = await query.ToListAsync();
					Dictionary<string, SQLiteRecordModel> localDbHashDictionary = new Dictionary<string, SQLiteRecordModel>();
					List<SQLiteRecordModel> localRecordsToUpdate = new List<SQLiteRecordModel>();
					List<SQLiteRecordModel> localRecordsToInsert = new List<SQLiteRecordModel>();

					_connectionToExternalCopy = new SQLiteAsyncConnection(externalDbLocalCopyPath);

					query = _connectionToExternalCopy.Table<SQLiteRecordModel>();
					List<SQLiteRecordModel> allExternalDbRecords = await query.ToListAsync();

					allLocalDbRecords.ForEach(r => localDbHashDictionary[r.Hash] = r);

					foreach (SQLiteRecordModel externalRecord in allExternalDbRecords)
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

				await externalStorage.UploadFile(originalLocalDbPath, externalDbPath);

				Debug.Log("External DB is Replaced by Local !");
				return true;
			}

			catch (Exception ex)
			{
				Debug.LogError($"There is an error while sync, trying to rollback to backup. ({ex.Message})\n{ex.StackTrace}");

				try
				{
					File.Copy(backupDbPath, originalLocalDbPath, true);
					Debug.Log("successful rollback!");
				}

				catch (Exception innerEx)
				{
					Debug.LogError($"There is some error while rolling back to backup. ({innerEx.Message})\n{innerEx.StackTrace}");
					_rollbackError = true;
					return false;
				}
			}

			return false;
		}

		private SQLiteRecordModel GetSQLiteRecordModel(DiaryRecordViewModel diaryViewModel)
		{
			int type = (int)diaryViewModel.Type;
			DateTime created = DateTime.Now;

			var sqliteRecord = new SQLiteRecordModel()
			{
				Id = diaryViewModel.Id,
				Type = type,
				RecordDateTime = diaryViewModel.DateTime,
				Content = diaryViewModel.Text,
				Created = created,
				Hash = Utils.GetSHA256Hash(
					type +
					diaryViewModel.DateTime.ToString(CultureInfo.InvariantCulture) +
					diaryViewModel.Text +
					created.ToString(CultureInfo.InvariantCulture)
				),
				AdditionalInfo = $"buildGUID={Application.buildGUID}"
			};

			return sqliteRecord;
		}

		private DiaryRecordViewModel GetDiaryRecordViewModel(SQLiteRecordModel sqliteRecord)
		{
			return new DiaryRecordViewModel(sqliteRecord.Id, sqliteRecord.RecordDateTime, sqliteRecord.Content);
		}

	}
}
