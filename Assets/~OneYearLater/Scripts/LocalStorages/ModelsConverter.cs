using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using OneYearLater.LocalStorages.Models;
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
				case Management.ERecordKey.Conversation:
					return ConvertToSQLiteRecordModelFrom((ConversationRecordViewModel)recordVM);
			}

			throw new Exception("invalid conversation");
		}

		public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(ConversationRecordViewModel conversationVM)
		{
			string conversationContent = JsonConvert.SerializeObject(conversationVM.Messages);
			return CreateSQLiteRecordModel(conversationVM, conversationContent);
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

		public static DiaryRecordViewModel ConvertToDiaryRecordViewModelFrom(SQLiteRecordModel sqliteRecord)
		{
			return new DiaryRecordViewModel(sqliteRecord.Id, sqliteRecord.RecordDateTime, sqliteRecord.Content);
		}
	}

}