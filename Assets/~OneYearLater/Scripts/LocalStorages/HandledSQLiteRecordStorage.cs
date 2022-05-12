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
		public UniTask Reconnect() => Handle(SqliteRecordStorage.Reconnect());
		public UniTask CloseAllConnections() => Handle(SqliteRecordStorage.CloseAllConnections());

		public UniTask<bool> IsDatabaseValid() => Handle(SqliteRecordStorage.IsDatabaseValid());


		protected override async UniTask<T> Handle<T>(UniTask<T> operation)
		{
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