using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.LocalStorage;
using SQLite;
using Zenject;

using static SQLite.SQLite3;

namespace OneYearLater.LocalStorages
{
	public class HandledSQLiteLocalRecordStorage : AbstractHandledLocalStorage
	{
		[Inject] protected SQLiteLocalRecordStorage _sqliteLocalRecordStorage;
		protected override ILocalRecordStorage LocalRecordStorage => _sqliteLocalRecordStorage;

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