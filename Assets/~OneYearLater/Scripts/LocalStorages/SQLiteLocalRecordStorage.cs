using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using UnityEngine;

using static Utilities.Utils;

using Debug = UnityEngine.Debug;

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
		private string _extarnalDbLocalCopyPostfix = "_external";



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

			var query = _connectionToLocal.Table<SQLiteRecordModel>().Where(r => (r.RecordDateTime >= dayStartInc) && (r.RecordDateTime < dayEndExc) && (!r.IsDeleted)).OrderBy(r => r.RecordDateTime);
			return (await query.ToListAsync())
				.Select(rm => new DiaryRecordViewModel(rm.RecordDateTime, rm.Content));
		}

		public UniTask SaveRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			return UniTask.CompletedTask;
		}

		public async UniTask<bool> SyncLocalAndExternalRecordStorages(IExternalRecordStorage externalStorage)
		{
			string originalLocalDbPath = LocalStorageUtils.GetDbPathOnDevice(_dbNameWithExtension);

			string externalDbPath = $"/{_dbNameWithExtension}";

			string externalDbLocalCopyNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(_dbNameWithExtension)}{_extarnalDbLocalCopyPostfix}{ Path.GetExtension(_dbNameWithExtension)}";

			string externalDbLocalCopyPath = LocalStorageUtils.GetDbPathOnDevice(externalDbLocalCopyNameWithExtension);

			string dbBackupNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(_dbNameWithExtension)}{_backupPostfix}{ Path.GetExtension(_dbNameWithExtension)}";

			string backupDbPath = LocalStorageUtils.GetDbPathOnDevice(dbBackupNameWithExtension);

			// Debug.Log($"originalLocalDbPath: {originalLocalDbPath}");
			// Debug.Log($"externalDbPath: {externalDbPath}");
			// Debug.Log($"externalDbLocalCopyNameWithExtension: {externalDbLocalCopyNameWithExtension}");
			// Debug.Log($"externalDbLocalCopyPath: {externalDbLocalCopyPath}");
			// Debug.Log($"dbBackupNameWithExtension: {dbBackupNameWithExtension}");
			// Debug.Log($"backupDbPath: {backupDbPath}");

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
					Debug.LogError($"There is an error while creating backup, sync aborted, try later. ({ex.Message}\n{ex.StackTrace})");
					return false;
				}
			}

			try
			{
				if (isExternalDbFileExisted)
				{
					await externalStorage.DownloadFile(externalDbPath, externalDbLocalCopyPath);

					_connectionToLocal = new SQLiteAsyncConnection(originalLocalDbPath);

					AsyncTableQuery<SQLiteRecordModel> query;

					query = _connectionToLocal.Table<SQLiteRecordModel>();
					List<SQLiteRecordModel> allLocalDbRecords = await query.ToListAsync();
					Dictionary<string, SQLiteRecordModel> localDbHashDictionary = new Dictionary<string, SQLiteRecordModel>();
					List<SQLiteRecordModel> localRecordsToUpdate = new List<SQLiteRecordModel>();
					List<SQLiteRecordModel> localRecordsToInsert = new List<SQLiteRecordModel>();

					_connectionToExternalCopy = new SQLiteAsyncConnection(externalDbLocalCopyPath);

					query = _connectionToExternalCopy.Table<SQLiteRecordModel>();
					List<SQLiteRecordModel> allExternalDbRecords = await query.ToListAsync();

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

				await externalStorage.UploadFile(originalLocalDbPath, externalDbPath);

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
