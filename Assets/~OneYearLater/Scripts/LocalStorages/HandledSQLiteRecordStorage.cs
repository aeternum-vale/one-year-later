using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.LocalStorage;
using SQLite;
using Zenject;

using static SQLite.SQLite3;

namespace OneYearLater.LocalStorages
{
	public class HandledSQLiteRecordStorage : AbstractHandledLocalStorage
	{
		[Inject] protected SQLiteRecordStorage SqliteRecordStorage;
		protected override ILocalRecordStorage LocalRecordStorage => SqliteRecordStorage;
		public UniTask Reconnect() => SqliteRecordStorage.Reconnect();
		public UniTask CloseAllConnections() => SqliteRecordStorage.CloseAllConnections();
		public UniTask<bool> IsDatabaseValid() => SqliteRecordStorage.IsDatabaseValid();


		public bool IsBusy { get; set; } = false;

		
		protected override async UniTask<T> Handle<T>(UniTask<T> operation)
		{
			if (IsBusy)
				await UniTask.WaitUntil(() => !IsBusy);
			
			try
			{
				T result = await operation;
				return result;
			}
			catch (SQLiteException ex)
			{
				if (ex.Result == Result.Corrupt ||
					ex.Result == Result.CannotOpen ||
					ex.Result == Result.NonDBFile ||
					ex.Result == Result.NotFound)
					throw new CannotAccessLocalStorageException(ex.Message, ex);

				throw new LocalStorageException(ex.Message, ex);
			}
		}

	}
}