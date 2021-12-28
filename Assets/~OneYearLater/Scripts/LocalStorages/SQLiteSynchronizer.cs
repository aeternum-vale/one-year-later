using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management.Interfaces;
using SQLite;
using UnityEngine;
using Zenject;

using static OneYearLater.LocalStorages.Constants;
using static OneYearLater.LocalStorages.Utils;

namespace OneYearLater.LocalStorages
{
	public class SQLiteSynchronizer : IRecordStorageSynchronizer
	{
		[Inject] private RecordStorageConnector _recordStorageConnector;

		private SQLiteAsyncConnection _connectionToExternalCopy;


		private IExternalStorage _externalStorage;

		private string _originalLocalDbPath;
		private string _externalDbPath;
		private string _externalDbLocalCopyNameWithExtension;
		private string _externalDbLocalCopyPath;
		private string _dbBackupNameWithExtension;
		private string _backupDbPath;


		private bool _isExternalDbFileExisted;
		private bool _isRollbackError = false;
		private bool _isSyncInProcess = false;


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

		private UniTask WaitUntilSyncIsNotInProcess()
		{
			if (_isSyncInProcess)
				return UniTask.WaitUntil(() => !_isSyncInProcess);
			return UniTask.CompletedTask;
		}

		public async UniTask<bool> TrySyncLocalAndExternalRecordStorages(IExternalStorage externalStorage)
		{
			await WaitUntilSyncIsNotInProcess();

			_isSyncInProcess = true;
			await _recordStorageConnector.OccupyConnectionBy(this);

			_externalStorage = externalStorage;
			_isExternalDbFileExisted = await _externalStorage.IsFileExist(_externalDbPath);

			//await UniTask.Delay(TimeSpan.FromSeconds(10f), DelayType.Realtime);

			bool? isSuccess = null;

			if (await IsLocalDbMustAndCanBeRestored())
				isSuccess = await TryRestoreLocalDb();

			if (_isExternalDbFileExisted && isSuccess == null)
			{
				bool isBackupCreated = TryCreateBackup();
				if (!isBackupCreated)
					isSuccess = false;
			}

			if (isSuccess == null)
				isSuccess = await TrySync();

			_isSyncInProcess = false;
			_recordStorageConnector.DeoccupyConnectionBy(this);
			return isSuccess.Value;
		}

		private async UniTask<bool> IsLocalDbMustAndCanBeRestored()
		{
			bool isLocalDbFileExisted = File.Exists(_originalLocalDbPath);
			bool isDatabaseValid = false;

			if (isLocalDbFileExisted)
				isDatabaseValid = await _recordStorageConnector.IsDatabaseValid();

			return (!isLocalDbFileExisted || !isDatabaseValid) && _isExternalDbFileExisted;
		}

		private async UniTask<bool> TryRestoreLocalDb()
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> TryRestoreLocalDb");
			try
			{
				await _recordStorageConnector.CloseConnectionBy(this);

				await _externalStorage.DownloadFile(_externalDbPath, _originalLocalDbPath);
				Debug.Log($"<color=lightblue>{GetType().Name}:</color> db restored!");

				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"Error while restoring missing local db ({ex.Message})\n{ex.StackTrace}");
				return false;
			}
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
				Debug.LogError($"Error while creating backup ({ex.Message})\n{ex.StackTrace}");
				return false;
			}
		}

		private async UniTask<bool> TrySync()
		{
			try
			{
				var conn = await _recordStorageConnector.GetConnectionFor(this);

				await MarkAllRecordsAsNonLocal();

				if (_isExternalDbFileExisted)
				{
					await _externalStorage.DownloadFile(_externalDbPath, _externalDbLocalCopyPath);
					await ApplyToLocalDbChangesFromExternal();
				}

				await CloseAllConnections();
				await _externalStorage.UploadFile(_originalLocalDbPath, _externalDbPath);

				Debug.Log("External DB is Replaced by Local!");

				return true;
			}

			catch (Exception ex)
			{
				Debug.LogError($"Error while sync, trying to rollback to backup. ({ex.Message})\n{ex.StackTrace}");
				TryRollbackToBackup();
				return false;
			}
		}

		private async UniTask ApplyToLocalDbChangesFromExternal()
		{
			var connectionToLocal = await _recordStorageConnector.GetConnectionFor(this);

			var query = connectionToLocal.Table<SQLiteRecordModel>();
			List<SQLiteRecordModel> allLocalDbRecords = await query.ToListAsync();
			Dictionary<string, SQLiteRecordModel> localDbHashDictionary =
				new Dictionary<string, SQLiteRecordModel>();
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

			var updatedRowsNumber = await connectionToLocal.UpdateAllAsync(localRecordsToUpdate);
			var insertedRowsNumber = await connectionToLocal.InsertAllAsync(localRecordsToInsert);
		}

		private async UniTask MarkAllRecordsAsNonLocal()
		{
			var connectionToLocal = await _recordStorageConnector.GetConnectionFor(this);

			await connectionToLocal
				.ExecuteAsync(
					$"UPDATE {nameof(SQLiteRecordModel)} SET {nameof(SQLiteRecordModel.IsLocal)} = 0"
				);
		}

		private async UniTask CloseAllConnections()
		{
			await _recordStorageConnector.CloseConnectionBy(this);

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
				Debug.LogError($"Error while rolling back ({ex.Message})\n{ex.StackTrace}");
				_isRollbackError = true;
				return false;
			}
		}

		private void RollbackToBackup()
		{
			File.Copy(_backupDbPath, _originalLocalDbPath, true);
		}

	}
}