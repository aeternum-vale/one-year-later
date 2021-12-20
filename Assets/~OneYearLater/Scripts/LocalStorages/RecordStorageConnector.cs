using System;
using Cysharp.Threading.Tasks;
using SQLite;
using UnityEngine;

using static OneYearLater.LocalStorages.Constants;
using static OneYearLater.LocalStorages.Utils;

public class RecordStorageConnector
{
	private SQLiteAsyncConnection _connection;
	private object _occupier;


	public async UniTask CloseConnection(object requester)
	{
		if (_occupier != null && _occupier != requester)
			await WaitUntilConnectionIsFree();

		if (_connection != null)
			await _connection.CloseAsync();

		_connection = null;
	}

	public async UniTask OccupyConnection(object requester)
	{
		if (_occupier == requester)
			throw new Exception("connection can't be occupied twice");

		await WaitUntilConnectionIsFree();
		_occupier = requester;
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> Connection is <color=yellow>occupied</color> by {requester}...");
	}

	public void DeoccupyConnection(object requester)
	{
		if (_occupier == requester)
		{
			_occupier = null;
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> Connection <color=green>deoccupied</color> by {requester}.");
			return;
		}

		throw new Exception("deoccupying before occupying");
	}

	public UniTask WaitUntilConnectionIsFree()
	{
		return UniTask.WaitUntil(() => _occupier == null);
	}


	public async UniTask<SQLiteAsyncConnection> GetConnectionFor(object requester)
	{
		if (_occupier != requester)
			await WaitUntilConnectionIsFree();

		if (_connection == null)
			_connection = new SQLiteAsyncConnection(GetDbPathOnDevice(RecordsDbNameWithExtension), SQLiteOpenFlags.ReadWrite);

		return _connection;
	}
}