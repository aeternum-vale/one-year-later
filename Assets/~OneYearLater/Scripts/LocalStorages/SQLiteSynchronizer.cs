
using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management.Interfaces;
using SQLite;
using UnityEngine;

using static OneYearLater.LocalStorages.Constants;
using static OneYearLater.LocalStorages.Utils;

namespace OneYearLater.LocalStorages
{
	public class SQLiteSynchronizer : IRecordStorageSynchronizer
	{
		private SQLiteAsyncConnection _connectionToLocal;
		private SQLiteAsyncConnection _connectionToExternalCopy;

		private string _originalLocalDbPath;
		private string _externalDbPath;
		private string _externalDbLocalCopyNameWithExtension;
		private string _externalDbLocalCopyPath;
		private string _dbBackupNameWithExtension;
		private string _backupDbPath;

		private IExternalStorage _externalStorage;
		private bool _isExternalDbFileExisted;
		private bool _rollbackError = false;

		public SQLiteSynchronizer()
		{
			_originalLocalDbPath = GetDbPathOnDevice(RecordsDbNameWithExtension);

			_externalDbPath = $"/{RecordsDbNameWithExtension}";

			_externalDbLocalCopyNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(RecordsDbNameWithExtension)}{ExternalDbLocalCopyPostfix}{Path.GetExtension(RecordsDbNameWithExtension)}";

			_externalDbLocalCopyPath = GetDbPathOnDevice(_externalDbLocalCopyNameWithExtension);

			_dbBackupNameWithExtension =
				$"{Path.GetFileNameWithoutExtension(RecordsDbNameWithExtension)}{BackupPostfix}{Path.GetExtension(RecordsDbNameWithExtension)}";

			_backupDbPath = GetDbPathOnDevice(_dbBackupNameWithExtension);
		}

		public async UniTask<bool> SyncLocalAndExternalRecordStoragesAsync(IExternalStorage externalStorage)
		{
			_externalStorage = externalStorage;
			_isExternalDbFileExisted = await _externalStorage.IsFileExist(_externalDbPath);

			if (_isExternalDbFileExisted)
			{
				bool isBackupCreated = TryCreateBackup();
				if (!isBackupCreated)
					return false;
			}

			return await TrySync();
		}

		private bool TryCreateBackup()
		{
			try
			{
				File.Copy(_originalLocalDbPath, _backupDbPath, true);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"There is an error while creating backup ({ex.Message})\n{ex.StackTrace}");
				return false;
			}
		}

		private async UniTask<bool> TrySync()
		{
			try
			{
				if (_isExternalDbFileExisted)
					await ApplyToLocalDbChangesFrom(_externalStorage);

				await CloseAllConnections();
				await _externalStorage.UploadFile(_originalLocalDbPath, _externalDbPath);

				Debug.Log("External DB is Replaced by Local!");
				return true;
			}

			catch (Exception ex)
			{
				Debug.LogError($"There is an error while sync, trying to rollback to backup. ({ex.Message})\n{ex.StackTrace}");
				TryRollbackToBackup();
				return false;
			}
		}

		private async UniTask ApplyToLocalDbChangesFrom(IExternalStorage externalStorage)
		{
			await externalStorage.DownloadFile(_externalDbPath, _externalDbLocalCopyPath);

			_connectionToLocal = new SQLiteAsyncConnection(_originalLocalDbPath);

			var query = _connectionToLocal.Table<SQLiteRecordModel>();
			List<SQLiteRecordModel> allLocalDbRecords = await query.ToListAsync();
			Dictionary<string, SQLiteRecordModel> localDbHashDictionary = new Dictionary<string, SQLiteRecordModel>();
			List<SQLiteRecordModel> localRecordsToUpdate = new List<SQLiteRecordModel>();
			List<SQLiteRecordModel> localRecordsToInsert = new List<SQLiteRecordModel>();

			_connectionToExternalCopy = new SQLiteAsyncConnection(_externalDbLocalCopyPath);

			query = _connectionToExternalCopy.Table<SQLiteRecordModel>();
			List<SQLiteRecordModel> allExternalDbRecords = await query.ToListAsync();

			allLocalDbRecords.ForEach(r => localDbHashDictionary[r.Hash] = r);

			foreach (SQLiteRecordModel externalRecord in allExternalDbRecords)
			{
				if (localDbHashDictionary.ContainsKey(externalRecord.Hash))
				{
					var localRecord = localDbHashDictionary[externalRecord.Hash];

					bool isExternalNewer = externalRecord.LastEdited > localRecord.LastEdited;

					if (isExternalNewer)
						localRecord.LastEdited = externalRecord.LastEdited;
					else
						continue;

					localRecord.Content = externalRecord.Content;
					localRecord.RecordDateTime = externalRecord.RecordDateTime;
					localRecord.IsDeleted = externalRecord.IsDeleted;

					localRecordsToUpdate.Add(localRecord);

					continue;
				}

				localRecordsToInsert.Add(externalRecord);
			}

			var updatedRowsNumber = await _connectionToLocal.UpdateAllAsync(localRecordsToUpdate);
			var insertedRowsNumber = await _connectionToLocal.InsertAllAsync(localRecordsToInsert);
		}

		private async UniTask CloseAllConnections()
		{
			if (_connectionToLocal != null)
				await _connectionToLocal.CloseAsync();
			if (_connectionToExternalCopy != null)
				await _connectionToExternalCopy.CloseAsync();
		}

		private bool TryRollbackToBackup()
		{
			try
			{
				RollbackToBackup();
				Debug.Log("successful rollback!");
				return true;
			}

			catch (Exception ex)
			{
				Debug.LogError($"There is some error while rolling back to backup. ({ex.Message})\n{ex.StackTrace}");
				_rollbackError = true;
				return false;
			}
		}

		private void RollbackToBackup()
		{
			File.Copy(_backupDbPath, _originalLocalDbPath, true);
		}

	}
}