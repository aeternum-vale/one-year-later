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
		[SerializeField] private string _dbNameWithExtension = "app.bytes";


		public SQLiteAppLocalStorage()
		{
			string dbPath = LocalStorageUtils.GetDbPathOnDevice(_dbNameWithExtension);
			_connection = new SQLiteAsyncConnection(dbPath);

			_connection.CreateTableAsync<SQLiteExternalStorageStateModel>().Forget();
		}

		public async UniTask<string> GetExternalStorageStateAsync(EExternalStorageKey key)
		{
			SQLiteExternalStorageStateModel stateModel = await _connection
				.Table<SQLiteExternalStorageStateModel>()
				.Where(s => s.Id == (int)key)
				.FirstOrDefaultAsync();
				
			return stateModel?.State ?? string.Empty;
		}

		public UniTask SaveExternalStorageStateAsync(ExternalStorageModel state)
		{
			
			SQLiteExternalStorageStateModel stateModel =
				new SQLiteExternalStorageStateModel() { Id = (int)state.key, State = state.serializedData };
			return _connection.InsertOrReplaceAsync(stateModel);
		}
	}
}
