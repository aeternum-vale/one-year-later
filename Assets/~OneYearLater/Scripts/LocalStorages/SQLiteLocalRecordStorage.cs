using System.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using UnityEngine;
using Zenject;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

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
				.Select(rm => ConvertToDiaryRecordViewModelFrom(rm));
		}

		public async UniTask<BaseRecordViewModel> GetRecordAsync(int recordId)
		{
			var sqliteRecord =
				await _connection.Table<SQLiteRecordModel>()
					.Where(r => (r.Id == recordId) && (!r.IsDeleted))
					.FirstAsync();

			return ConvertToDiaryRecordViewModelFrom(sqliteRecord);
		}

		public async UniTask InsertRecordAsync(BaseRecordViewModel record)
		{
			switch (record.Type)
			{
				case ERecordKey.Diary:

					DiaryRecordViewModel diaryRecordVM = (DiaryRecordViewModel)record;
					SQLiteRecordModel diaryRecordModel = ConvertToSQLiteRecordModelFrom(diaryRecordVM);

					var existedCount = await _connection.Table<SQLiteRecordModel>()
						.Where(r => r.Content.Equals(diaryRecordVM.Text) && r.RecordDateTime == diaryRecordVM.DateTime)
						.CountAsync();
					if (existedCount > 0)
						throw new RecordDuplicateException();

					await _connection.InsertAsync(diaryRecordModel);
					break;
				default: throw new Exception("invalid record type");
			}
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

		private SQLiteRecordModel ConvertToSQLiteRecordModelFrom(DiaryRecordViewModel diaryViewModel)
		{
			int type = (int)diaryViewModel.Type;
			DateTime now = DateTime.Now;

			StringBuilder stringForHashingBuilder = new StringBuilder();
			stringForHashingBuilder.Append(type);
			stringForHashingBuilder.Append(diaryViewModel.DateTime);
			stringForHashingBuilder.Append(diaryViewModel.Text);
			if (!diaryViewModel.IsImported) 
				stringForHashingBuilder.Append(now.ToString(CultureInfo.InvariantCulture));

			var sqliteRecord = new SQLiteRecordModel()
			{
				Id = diaryViewModel.Id,
				Type = type,
				RecordDateTime = diaryViewModel.DateTime,
				Content = diaryViewModel.Text,
				Created = now,
				LastEdited = now,
				IsLocal = true,
				Hash = Utilities.Utils.GetSHA256Hash(stringForHashingBuilder.ToString()),
				AdditionalInfo = $"buildGUID={Application.buildGUID}"
			};

			return sqliteRecord;
		}

		private DiaryRecordViewModel ConvertToDiaryRecordViewModelFrom(SQLiteRecordModel sqliteRecord)
		{
			return new DiaryRecordViewModel(sqliteRecord.Id, sqliteRecord.RecordDateTime, sqliteRecord.Content);
		}

	}
}
