using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using Zenject;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

using static OneYearLater.LocalStorages.ModelsConverter;

namespace OneYearLater.LocalStorages
{
	public class SQLiteLocalRecordStorage : ILocalRecordStorage
	{
		[Inject] private RecordStorageConnector _recordStorageConnector;
		private SQLiteAsyncConnection _connection;


		public async UniTask<EInitResult> Init()
		{
			var result = await _recordStorageConnector.InitDatabase();
			await Reconnect();
			return result;
		}

		public async UniTask Reconnect()
		{
			_connection = await _recordStorageConnector.GetReadWriteConnection();
		}

		public async UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
		{
			var readOnlyConnection = await _recordStorageConnector.GetReadOnlyConnection();

			DateTime dayStartInc = date.Date;
			DateTime dayEndExc = date.Date.AddDays(1);

			var query =
				readOnlyConnection.Table<SQLiteRecordModel>()
					.Where(r => r.RecordDateTime >= dayStartInc && (r.RecordDateTime < dayEndExc) && (!r.IsDeleted))
					.OrderBy(r => r.RecordDateTime);

			return (await query.ToListAsync())
				.Select(rm => ConvertTRecordViewModelFrom(rm));
		}

		public async UniTask<BaseRecordViewModel> GetRecordAsync(int recordId)
		{
			var sqliteRecord =
				await _connection.Table<SQLiteRecordModel>()
					.Where(r => (r.Id == recordId) && (!r.IsDeleted))
					.FirstAsync();

			return ConvertTRecordViewModelFrom(sqliteRecord);
		}

		public async UniTask InsertRecordAsync(BaseRecordViewModel recordVM)
		{
			SQLiteRecordModel recordModel = ConvertToSQLiteRecordModelFrom(recordVM);

			var existedCount = await _connection.Table<SQLiteRecordModel>()
				.Where(r => r.Content.Equals(recordModel.Content) && r.RecordDateTime == recordModel.RecordDateTime)
				.CountAsync();
			if (existedCount > 0)
				throw new RecordDuplicateException();

			await _connection.InsertAsync(recordModel);
		}

		public async UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			List<SQLiteRecordModel> sqliteRecordModels = new List<SQLiteRecordModel>();
			foreach (var record in records)
				switch (record.Type)
				{
					case ERecordKey.Diary:
						sqliteRecordModels.Add(ConvertToSQLiteRecordModelFrom((DiaryRecordViewModel)record));
						break;
				}

			await _connection.InsertAllAsync(sqliteRecordModels);
		}

		public async UniTask UpdateRecordAsync(BaseRecordViewModel recordVM)
		{
			switch (recordVM.Type)
			{
				case ERecordKey.Diary:

					var diaryVM = (DiaryRecordViewModel)recordVM;

					var record = await RetrieveSQLiteRecordModelBy(diaryVM.Id);

					record.RecordDateTime = diaryVM.DateTime;
					record.Content = diaryVM.Text;
					record.LastEdited = DateTime.Now;

					await _connection.UpdateAsync(record);

					break;

				default: throw new Exception("invalid record type");
			}
		}

		public async UniTask DeleteRecordAsync(int recordId)
		{
			var sqliteRecord = await RetrieveSQLiteRecordModelBy(recordId);

			if (sqliteRecord.IsLocal)
			{
				await _connection.DeleteAsync(sqliteRecord);
			}
			else
			{
				sqliteRecord.IsDeleted = true;
				sqliteRecord.LastEdited = DateTime.Now;
				await _connection.UpdateAsync(sqliteRecord);
			}
		}

		private async UniTask<SQLiteRecordModel> RetrieveSQLiteRecordModelBy(int id)
		{
			return await _connection.Table<SQLiteRecordModel>().Where(r => r.Id == id).FirstAsync();
		}

	}
}
