using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.ViewModels;
using UnityEngine;

namespace OneYearLater.LocalStorages
{
	public static class ModelsConverter
	{
		public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(BaseRecordViewModel recordVM)
		{
			switch (recordVM.Type)
			{
				case Management.ERecordKey.Diary:
					return ConvertToSQLiteRecordModelFrom((DiaryRecordViewModel)recordVM);
				case Management.ERecordKey.Message:
					return ConvertToSQLiteRecordModelFrom((MessageRecordViewModel)recordVM);
			}

			throw new Exception("invalid record");
		}

		public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(MessageRecordViewModel messageVM)
		{
			string messageContent = JsonConvert.SerializeObject(messageVM.Content);
			return CreateSQLiteRecordModel(messageVM, messageContent);
		}

		public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(DiaryRecordViewModel diaryVM)
		{
			return CreateSQLiteRecordModel(diaryVM, diaryVM.Text);
		}

		private static SQLiteRecordModel CreateSQLiteRecordModel(BaseRecordViewModel recordVM, string content)
		{
			int type = (int)recordVM.Type;
			DateTime now = DateTime.Now;

			StringBuilder stringForHashingBuilder = new StringBuilder();
			stringForHashingBuilder.Append(type);
			stringForHashingBuilder.Append(recordVM.DateTime);
			stringForHashingBuilder.Append(content);
			if (!recordVM.IsImported)
				stringForHashingBuilder.Append(now.ToString(CultureInfo.InvariantCulture));

			var sqliteRecord = new SQLiteRecordModel()
			{
				Id = recordVM.Id,
				Type = type,
				RecordDateTime = recordVM.DateTime,
				Content = content,
				Created = now,
				LastEdited = now,
				IsLocal = true,
				Hash = Utilities.Utils.GetSHA256Hash(stringForHashingBuilder.ToString()),
				AdditionalInfo = $"buildGUID={Application.buildGUID}"
			};

			return sqliteRecord;
		}

		public static BaseRecordViewModel ConvertTRecordViewModelFrom(SQLiteRecordModel sqliteRecord)
		{
			ERecordKey type = (ERecordKey)sqliteRecord.Type;

			switch (type)
			{
				case ERecordKey.Diary:
					return new DiaryRecordViewModel(sqliteRecord.Id, sqliteRecord.RecordDateTime, sqliteRecord.Content);
				case ERecordKey.Message:
					var mvm = new MessageRecordViewModel(sqliteRecord.Id, sqliteRecord.RecordDateTime);
					mvm.Content = JsonConvert.DeserializeObject<MessageContent>(sqliteRecord.Content);
					return mvm;
			}

			throw new Exception("invalid record");


		}
	}

}