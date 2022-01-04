using System;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;

using static OneYearLater.LocalStorages.Constants;
using static OneYearLater.LocalStorages.Utils;

namespace OneYearLater.LocalStorages
{
	public class SQLiteAppLocalStorage : IAppLocalStorage
	{

		private SQLiteAsyncConnection _connection;
		

		public SQLiteAppLocalStorage()
		{
			string dbPath = GetDbPathOnDevice(AppDbNameWithExtension);
			_connection = new SQLiteAsyncConnection(dbPath);

			_connection.CreateTableAsync<SQLiteExternalStorageModel>().Forget();
		}

		public async UniTask<ExternalStorageViewModel?> GetExternalStorageViewModel(EExternalStorageKey key)
		{
			SQLiteExternalStorageModel sqliteModel = await _connection
				.Table<SQLiteExternalStorageModel>()
				.Where(s => s.Id == (int)key)
				.FirstOrDefaultAsync();

			return sqliteModel != null ?
				new ExternalStorageViewModel
				{
					key = key,
					state = sqliteModel.State,
					lastSync = sqliteModel.LastSync
				} :
				(ExternalStorageViewModel?)null;
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
