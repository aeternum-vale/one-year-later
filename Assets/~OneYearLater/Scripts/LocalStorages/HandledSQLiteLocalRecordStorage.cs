using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.LocalStorage;
using SQLite;
using Zenject;

namespace OneYearLater.LocalStorages
{
	public class HandledSQLiteLocalRecordStorage : AbstractHandledLocalStorage
	{
		[Inject] protected SQLiteLocalRecordStorage _sqliteLocalRecordStorage;
		protected override ILocalRecordStorage LocalRecordStorage => _sqliteLocalRecordStorage;

		protected override async UniTask Handle(UniTask operation)
		{
			try
			{
				await operation;
			}
			catch (SQLiteException ex)
			{
				throw new LocalStorageException("LocalStorageException", ex);
			}
		}

		protected override async UniTask<T> Handle<T>(UniTask<T> operation)
		{
			try
			{
				T result = await operation;
				return result;
			}
			catch (SQLiteException ex)
			{
				throw new LocalStorageException("LocalStorageException", ex);
			}
		}

	}
}