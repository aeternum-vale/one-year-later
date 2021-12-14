using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using UnityEngine;

using static OneYearLater.LocalStorages.Constants;
using static OneYearLater.LocalStorages.Utils;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif
namespace OneYearLater.LocalStorages
{
	public class SQLiteLocalRecordStorage : ILocalRecordStorage
	{
		private SQLiteAsyncConnection _conn;

		public SQLiteLocalRecordStorage()
		{
			string localDbPath = GetDbPathOnDevice(RecordsDbNameWithExtension);
			_conn = new SQLiteAsyncConnection(localDbPath);

			_conn.CreateTableAsync<SQLiteRecordModel>().Forget();
		}

		public async UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
		{
			DateTime dayStartInc = date.Date;
			DateTime dayEndExc = date.Date.AddDays(1);

			var query =
				_conn.Table<SQLiteRecordModel>()
					.Where(r => r.RecordDateTime >= dayStartInc && (r.RecordDateTime < dayEndExc) && (!r.IsDeleted))
					.OrderBy(r => r.RecordDateTime);

			return (await query.ToListAsync())
				.Select(rm => GetDiaryRecordViewModel(rm));
		}

		public async UniTask<BaseRecordViewModel> GetRecordAsync(int recordId)
		{
			var sqliteRecord =
				await _conn.Table<SQLiteRecordModel>()
					.Where(r => (r.Id == recordId) && (!r.IsDeleted))
					.FirstAsync();

			return GetDiaryRecordViewModel(sqliteRecord);
		}

		public UniTask InsertRecordAsync(BaseRecordViewModel record)
		{
			switch (record.Type)
			{
				case ERecordKey.Diary:
					return _conn.InsertAsync(GetSQLiteRecordModel((DiaryRecordViewModel)record));
				default:
					throw new Exception("invalid record type");
			}
		}

		public UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			List<SQLiteRecordModel> sqliteRecordModels = new List<SQLiteRecordModel>();
			foreach (var record in records)
				switch (record.Type)
				{
					case ERecordKey.Diary:
						sqliteRecordModels.Add(GetSQLiteRecordModel((DiaryRecordViewModel)record));
						break;
				}

			return _conn.InsertAllAsync(sqliteRecordModels);
		}

		public UniTask UpdateRecordAsync(BaseRecordViewModel record)
		{
			switch (record.Type)
			{
				case ERecordKey.Diary:
					return _conn.UpdateAsync(GetSQLiteRecordModel((DiaryRecordViewModel)record));
				default:
					throw new Exception("invalid record type");
			}
		}

		public async UniTask DeleteRecordAsync(int recordId)
		{
			var sqliteRecord =
				await _conn.Table<SQLiteRecordModel>()
					.Where(r => r.Id == recordId)
					.FirstAsync();

			if (sqliteRecord.IsLocal)
			{
				await _conn.DeleteAsync(sqliteRecord);
			}
			else
			{
				sqliteRecord.IsDeleted = true;
				await _conn.UpdateAsync(sqliteRecord);
			}
		}

		private SQLiteRecordModel GetSQLiteRecordModel(DiaryRecordViewModel diaryViewModel)
		{
			int type = (int)diaryViewModel.Type;
			DateTime created = DateTime.Now;

			var sqliteRecord = new SQLiteRecordModel()
			{
				Id = diaryViewModel.Id,
				Type = type,
				RecordDateTime = diaryViewModel.DateTime,
				Content = diaryViewModel.Text,
				Created = created,
				IsLocal = true,
				Hash = Utilities.Utils.GetSHA256Hash(
					type +
					diaryViewModel.DateTime.ToString(CultureInfo.InvariantCulture) +
					diaryViewModel.Text +
					created.ToString(CultureInfo.InvariantCulture)
				),
				AdditionalInfo = $"buildGUID={Application.buildGUID}"
			};

			return sqliteRecord;
		}

		private DiaryRecordViewModel GetDiaryRecordViewModel(SQLiteRecordModel sqliteRecord)
		{
			return new DiaryRecordViewModel(sqliteRecord.Id, sqliteRecord.RecordDateTime, sqliteRecord.Content);
		}
	}
}
