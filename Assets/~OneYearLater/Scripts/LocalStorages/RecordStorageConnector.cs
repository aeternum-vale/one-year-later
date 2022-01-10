using System;
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
	public class RecordStorageConnector
	{
		private SQLiteAsyncConnection _readWriteConnection;
		private SQLiteAsyncConnection _readOnlyConnection;
		private bool _isDbInitiated = false;


		public async UniTask<EInitResult> InitDatabase()
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> InitDatabase");

			EInitResult? result = null;

			if (_isDbInitiated)
				throw new Exception("db has already been initiated");

			string dbPath = GetDbPathOnDevice(RecordsDbNameWithExtension);

			if (File.Exists(dbPath))
			{
				bool isDatabaseValid = await IsDatabaseValid();

				if (!isDatabaseValid)
				{
					File.Delete(dbPath);
					result = EInitResult.InvalidDatabase;
				}
			}
			else
				result = EInitResult.NoDatabase;

			var tempConnection = new SQLiteAsyncConnection(dbPath);
			await tempConnection.CreateTableAsync<SQLiteRecordModel>();
			await tempConnection.CloseAsync();

			_isDbInitiated = true;

			if (result == null)
				result = EInitResult.ValidDatabase;

			return result.Value;
		}

		public async UniTask<SQLiteAsyncConnection> GetReadOnlyConnection()
		{
			await WaitUntilDbInitiated();

			InitializeReadOnlyConnection();

			return _readOnlyConnection;
		}

		public async UniTask<SQLiteAsyncConnection> GetReadWriteConnection()
		{
			await WaitUntilDbInitiated();

			InitializeReadWriteConnection();

			return _readWriteConnection;
		}

		public async UniTask CloseAllConnections()
		{
			if (_readWriteConnection != null)
				await _readWriteConnection.CloseAsync();

			_readWriteConnection = null;

			if (_readOnlyConnection != null)
				await _readOnlyConnection.CloseAsync();

			_readOnlyConnection = null;
		}

		public async UniTask<bool> IsDatabaseValid()
		{
			SQLiteAsyncConnection conn = null;
			try
			{
				conn = OpenNewReadOnlyConnection();
				await conn.Table<SQLiteRecordModel>().FirstOrDefaultAsync();
				await conn.CloseAsync();
				return true;
			}

			catch (Exception) { return false; }

			finally
			{
				if (conn != null)
					await conn.CloseAsync();
			}
		}


		private UniTask WaitUntilDbInitiated()
		{
			if (_isDbInitiated)
				return UniTask.CompletedTask;

			return UniTask.WaitUntil(() => _isDbInitiated);
		}

		private SQLiteAsyncConnection OpenNewReadWriteConnection() =>
			OpenNewConnection(SQLiteOpenFlags.ReadWrite);

		private SQLiteAsyncConnection OpenNewReadOnlyConnection() =>
			OpenNewConnection(SQLiteOpenFlags.ReadOnly);

		private SQLiteAsyncConnection OpenNewConnection(SQLiteOpenFlags flag) =>
			new SQLiteAsyncConnection(GetDbPathOnDevice(RecordsDbNameWithExtension), flag);

		private void InitializeReadWriteConnection()
		{
			_readWriteConnection ??= OpenNewReadWriteConnection();
		}

		private void InitializeReadOnlyConnection()
		{
			_readOnlyConnection ??= OpenNewReadOnlyConnection();
		}
	}
}