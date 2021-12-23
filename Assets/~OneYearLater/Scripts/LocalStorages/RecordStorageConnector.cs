using System;
using System.IO;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using SQLite;
using UnityEngine;

using static OneYearLater.LocalStorages.Constants;
using static OneYearLater.LocalStorages.Utils;

public class RecordStorageConnector
{
	private SQLiteAsyncConnection _rwConnection;
	private object _occupier;
	private bool _isDbInitiated = false;

	public RecordStorageConnector()
	{
		InitDatabase().Forget();
	}

	private async UniTask InitDatabase()
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> InitDatabase");

		if (_isDbInitiated)
			throw new Exception("db has already been initiated");

		string dbPath = GetDbPathOnDevice(RecordsDbNameWithExtension);

		if (File.Exists(dbPath))
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> db file exists");
			bool isDatabaseValid = await IsDatabaseValid();

			if (!isDatabaseValid)
				File.Delete(dbPath);
		}

		var tempConnection = new SQLiteAsyncConnection(dbPath);
		await tempConnection.CreateTableAsync<SQLiteRecordModel>();
		await tempConnection.CloseAsync();

		_isDbInitiated = true;
	}

	public async UniTask CloseConnectionBy(object requester)
	{
		if (_occupier != null && _occupier != requester)
			await WaitUntilConnectionIsFree();

		await CloseConnection();
	}

	private async UniTask CloseConnection()
	{
		if (_rwConnection != null)
			await _rwConnection.CloseAsync();

		_rwConnection = null;
	}

	public async UniTask OccupyConnectionBy(object requester)
	{
		if (_occupier == requester)
			throw new Exception("connection can't be occupied twice");

		await WaitUntilConnectionIsFree();
		_occupier = requester;
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> Connection is <color=yellow>occupied</color> by {requester.GetType().Name}...");
	}

	public void DeoccupyConnectionBy(object requester)
	{
		if (_occupier == requester)
		{
			_occupier = null;
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> Connection <color=green>deoccupied</color> by {requester.GetType().Name}.");
			return;
		}

		throw new Exception("deoccupying before occupying");
	}

	private UniTask WaitUntilConnectionIsFree()
	{
		if (_occupier == null)
			return UniTask.CompletedTask;

		return UniTask.WaitUntil(() => _occupier == null);
	}

	private UniTask WaitUntilDbInitiated()
	{
		if (_isDbInitiated)
			return UniTask.CompletedTask;

		return UniTask.WaitUntil(() => _isDbInitiated);
	}

	public async UniTask<SQLiteAsyncConnection> GetConnectionFor(object requester)
	{
		await WaitUntilDbInitiated();

		if (_occupier != requester)
			await WaitUntilConnectionIsFree();

		InitializeReadWriteConnection();

		return _rwConnection;
	}

	public async UniTask<bool> IsDatabaseValid()
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> IsDatabaseValid()");
		SQLiteAsyncConnection conn = null;
		try
		{
			conn = GetNewReadWriteConnection();
			await conn.Table<SQLiteRecordModel>().FirstOrDefaultAsync();
			await conn.CloseAsync();

			Debug.Log($"<color=lightblue>{GetType().Name}:</color> IsDatabaseValid=true");
			return true;

		}
		catch (Exception) { return false; }

		finally
		{
			if (conn != null)
				await conn.CloseAsync();
		}
	}

	private SQLiteAsyncConnection GetNewReadWriteConnection()
	{
		return new SQLiteAsyncConnection(GetDbPathOnDevice(RecordsDbNameWithExtension), SQLiteOpenFlags.ReadWrite);
	}

	private void InitializeReadWriteConnection()
	{
		if (_rwConnection == null)
			_rwConnection = GetNewReadWriteConnection();

	}
}