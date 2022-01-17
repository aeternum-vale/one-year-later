using System;
using System.Globalization;
using System.Text;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.ViewModels;
using UnityEngine;

namespace OneYearLater.LocalStorages
{
	public static class ModelsConverter
	{
		public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(BaseRecordViewModel recordVM, int contentId)
		{
			switch (recordVM.Type)
			{
				case ERecordType.Diary:
					return ConvertToSQLiteRecordModelFrom((DiaryRecordViewModel)recordVM, contentId);
				case ERecordType.Message:
					return ConvertToSQLiteRecordModelFrom((MessageRecordViewModel)recordVM, contentId);
			}

			throw new Exception("invalid record");
		}

		public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(MessageRecordViewModel messageVM, int contentId)
		{
			StringBuilder contentBuilder = new StringBuilder();
			contentBuilder.Append(messageVM.MessageText);
			contentBuilder.Append(messageVM.IsFromUser);
			contentBuilder.Append(messageVM.ConversationalistName);
			return CreateSQLiteRecordModel(messageVM, contentBuilder.ToString(), contentId);
		}

		public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(DiaryRecordViewModel diaryVM, int contentId)
		{
			return CreateSQLiteRecordModel(diaryVM, diaryVM.Text, contentId);
		}

		private static SQLiteRecordModel CreateSQLiteRecordModel(BaseRecordViewModel recordVM, string contentToHash, int contentId)
		{
			int type = (int)recordVM.Type;
			DateTime now = DateTime.Now;

			StringBuilder stringForHashingBuilder = new StringBuilder();
			stringForHashingBuilder.Append(type);
			stringForHashingBuilder.Append(recordVM.DateTime);
			stringForHashingBuilder.Append(contentToHash);
			if (!recordVM.IsImported)
				stringForHashingBuilder.Append(now.ToString(CultureInfo.InvariantCulture));

			var sqliteRecord = new SQLiteRecordModel()
			{
				Id = recordVM.Id,
				Type = type,
				RecordDateTime = recordVM.DateTime,
				ContentId = contentId,
				Created = now,
				LastEdited = now,
				IsLocal = true,
				Hash = Utilities.Utils.GetSHA256Hash(stringForHashingBuilder.ToString()),
				AdditionalInfo = $"buildGUID={Application.buildGUID}"
			};

			return sqliteRecord;
		}



		public static DiaryRecordViewModel ConvertToRecordViewModelFrom(SQLiteRecordModel diarySqliteRecord, SQLiteDiaryContentModel diaryContent)
		{
			return new DiaryRecordViewModel(diarySqliteRecord.Id, diarySqliteRecord.RecordDateTime, diaryContent.Text);
		}

		public static MessageRecordViewModel ConvertToRecordViewModelFrom(SQLiteRecordModel messageSqliteRecord, SQLiteMessengeContentModel messageContentModel, string conversationalistName)
		{
			var messageRecordVM = new MessageRecordViewModel(messageSqliteRecord.Id, messageSqliteRecord.RecordDateTime);

			messageRecordVM.IsFromUser = messageContentModel.IsFromUser;
			messageRecordVM.MessageText = messageContentModel.MessageText;
			messageRecordVM.ConversationalistName = conversationalistName;

			return messageRecordVM;
		}

	}

}