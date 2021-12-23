using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UnityEngine;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

namespace OneYearLater.LocalStorages
{
	public class SQLiteLocalRecordStorage : ILocalRecordStorage
	{
		private RecordStorageConnector _recordStorageConnector;

		public SQLiteLocalRecordStorage(RecordStorageConnector recordStorageConnector)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> ctor");
			_recordStorageConnector = recordStorageConnector;
		}

		public async UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
		{
			var conn = await _recordStorageConnector.GetConnectionFor(this);

			DateTime dayStartInc = date.Date;
			DateTime dayEndExc = date.Date.AddDays(1);

			var query =
				conn.Table<SQLiteRecordModel>()
					.Where(r => r.RecordDateTime >= dayStartInc && (r.RecordDateTime < dayEndExc) && (!r.IsDeleted))
					.OrderBy(r => r.RecordDateTime);

			return (await query.ToListAsync())
				.Select(rm => ConvertToDiaryRecordViewModelFrom(rm));
		}

		public async UniTask<BaseRecordViewModel> GetRecordAsync(int recordId)
		{
			var conn = await _recordStorageConnector.GetConnectionFor(this);

			var sqliteRecord =
				await conn.Table<SQLiteRecordModel>()
					.Where(r => (r.Id == recordId) && (!r.IsDeleted))
					.FirstAsync();

			return ConvertToDiaryRecordViewModelFrom(sqliteRecord);
		}

		public async UniTask InsertRecordAsync(BaseRecordViewModel record)
		{
			var conn = await _recordStorageConnector.GetConnectionFor(this);

			switch (record.Type)
			{
				case ERecordKey.Diary:
					await conn.InsertAsync(ConvertToSQLiteRecordModelFrom((DiaryRecordViewModel)record));
				break;
				default: throw new Exception("invalid record type");
			}
		}

		public async UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			var conn = await _recordStorageConnector.GetConnectionFor(this);

			List<SQLiteRecordModel> sqliteRecordModels = new List<SQLiteRecordModel>();
			foreach (var record in records)
				switch (record.Type)
				{
					case ERecordKey.Diary:
						sqliteRecordModels.Add(ConvertToSQLiteRecordModelFrom((DiaryRecordViewModel)record));
						break;
				}

			await conn.InsertAllAsync(sqliteRecordModels);
		}

		public async UniTask UpdateRecordAsync(BaseRecordViewModel recordVM)
		{
			var conn = await _recordStorageConnector.GetConnectionFor(this);

			switch (recordVM.Type)
			{
				case ERecordKey.Diary:

					var diaryVM = (DiaryRecordViewModel)recordVM;

					var record = await RetrieveSQLiteRecordModelBy(diaryVM.Id);

					record.RecordDateTime = diaryVM.DateTime;
					record.Content = diaryVM.Text;
					record.LastEdited = DateTime.Now;

					await conn.UpdateAsync(record);

					break;
			
				default: throw new Exception("invalid record type");
			}
		}

		public async UniTask DeleteRecordAsync(int recordId)
		{
			var conn = await _recordStorageConnector.GetConnectionFor(this);

			var sqliteRecord = await RetrieveSQLiteRecordModelBy(recordId);

			if (sqliteRecord.IsLocal)
			{
				await conn.DeleteAsync(sqliteRecord);
			}
			else
			{
				sqliteRecord.IsDeleted = true;
				sqliteRecord.LastEdited = DateTime.Now;
				await conn.UpdateAsync(sqliteRecord);
			}
		}


		private async UniTask<SQLiteRecordModel> RetrieveSQLiteRecordModelBy(int id)
		{
			var conn = await _recordStorageConnector.GetConnectionFor(this);
			return await conn.Table<SQLiteRecordModel>().Where(r => r.Id == id).FirstAsync();
		}

		private SQLiteRecordModel ConvertToSQLiteRecordModelFrom(DiaryRecordViewModel diaryViewModel)
		{
			int type = (int)diaryViewModel.Type;
			DateTime now = DateTime.Now;

			var sqliteRecord = new SQLiteRecordModel()
			{
				Id = diaryViewModel.Id,
				Type = type,
				RecordDateTime = diaryViewModel.DateTime,
				Content = diaryViewModel.Text,
				Created = now,
				LastEdited = now,
				IsLocal = true,
				Hash = Utilities.Utils.GetSHA256Hash(
					type +
					diaryViewModel.DateTime.ToString(CultureInfo.InvariantCulture) +
					diaryViewModel.Text +
					now.ToString(CultureInfo.InvariantCulture)
				),
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
