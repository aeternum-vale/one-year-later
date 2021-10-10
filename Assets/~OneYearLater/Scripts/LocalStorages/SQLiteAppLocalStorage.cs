using System;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using SQLite;
using UnityEngine;

namespace OneYearLater.LocalStorages
{
	public class SQLiteAppLocalStorage : IAppLocalStorage
	{

		private SQLiteAsyncConnection _connection;
		private string _dbNameWithExtension = "app.bytes";


		public SQLiteAppLocalStorage()
		{
			string dbPath = LocalStorageUtils.GetDbPathOnDevice(_dbNameWithExtension);
			_connection = new SQLiteAsyncConnection(dbPath);

			_connection.CreateTableAsync<SQLiteExternalStorageModel>().Forget();
		}

		public async UniTask<ExternalStorageModel?> GetExternalStorageAsync(EExternalStorageKey key)
		{
			SQLiteExternalStorageModel sqliteModel = await _connection
				.Table<SQLiteExternalStorageModel>()
				.Where(s => s.Id == (int)key)
				.FirstOrDefaultAsync();

			return sqliteModel != null ?
				new ExternalStorageModel
				{
					key = key,
					state = sqliteModel.State,
					lastSync = sqliteModel.LastSync
				} :
				(ExternalStorageModel?)null;
		}

		public async UniTask UpdateExternalStorageStateAsync(EExternalStorageKey key, string state)
		{
			SQLiteExternalStorageModel dbModel = await _connection
				.Table<SQLiteExternalStorageModel>()
				.Where(s => s.Id == (int)key)
				.FirstOrDefaultAsync();

			SQLiteExternalStorageModel modelToInsert = dbModel ?? new SQLiteExternalStorageModel { Id = (int)key };

			modelToInsert.State = state;

			await _connection.InsertOrReplaceAsync(modelToInsert);
		}

		public async UniTask UpdateExternalStorageSyncDateAsync(EExternalStorageKey key, DateTime syncDate)
		{
			var dbModel = await _connection
				.Table<SQLiteExternalStorageModel>()
				.Where(s => s.Id == (int)key)
				.FirstOrDefaultAsync();

			var modelToInsert = dbModel ?? new SQLiteExternalStorageModel() { Id = (int)key };
			modelToInsert.LastSync = syncDate;

			await _connection.InsertOrReplaceAsync(modelToInsert);
		}

	}
}
