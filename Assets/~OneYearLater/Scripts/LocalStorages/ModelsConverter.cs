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
        public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(BaseRecordViewModel recordVM)
        {
            switch (recordVM.Type)
            {
                case ERecordType.Diary:
                    return ConvertToSQLiteRecordModelFrom((DiaryRecordViewModel) recordVM);
                case ERecordType.Message:
                    return ConvertToSQLiteRecordModelFrom((MessageRecordViewModel) recordVM);
            }

            throw new Exception("unknown record");
        }

        public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(MessageRecordViewModel messageVM)
        {
            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.Append(messageVM.MessageText);
            contentBuilder.Append(messageVM.IsFromUser);
            contentBuilder.Append(messageVM.Conversationalist.Name);
            return CreateSQLiteRecordModel(messageVM, contentBuilder.ToString());
        }

        public static SQLiteRecordModel ConvertToSQLiteRecordModelFrom(DiaryRecordViewModel diaryVM)
        {
            return CreateSQLiteRecordModel(diaryVM, diaryVM.Text);
        }

        private static SQLiteRecordModel CreateSQLiteRecordModel(BaseRecordViewModel recordVM, string contentToHash)
        {
            int type = (int) recordVM.Type;
            DateTime now = DateTime.Now;

            StringBuilder stringForHashingBuilder = new StringBuilder();
            stringForHashingBuilder.Append(type);
            stringForHashingBuilder.Append(recordVM.RecordDateTime);
            stringForHashingBuilder.Append(contentToHash);
            if (!recordVM.IsImported)
                stringForHashingBuilder.Append(now.ToString(CultureInfo.InvariantCulture));

            var sqliteRecord = new SQLiteRecordModel()
            {
                Type = type,
                RecordDateTime = recordVM.RecordDateTime,
                Created = now,
                LastEdited = now,
                IsLocal = true,
                Hash = Utilities.Utils.GetSHA256Hash(stringForHashingBuilder.ToString()),
                AdditionalInfo = $"buildGUID={Application.buildGUID}"
            };

            return sqliteRecord;
        }


        public static DiaryRecordViewModel ConvertToRecordViewModelFrom(SQLiteRecordModel diarySqliteRecord,
            SQLiteDiaryContentModel diaryContent)
        {
            return new DiaryRecordViewModel(diarySqliteRecord.Hash, diarySqliteRecord.RecordDateTime,
                diaryContent.Text);
        }

        public static MessageRecordViewModel ConvertToRecordViewModelFrom(SQLiteRecordModel messageSqliteRecord,
            SQLiteMessageContentModel messageContentModel, SQLiteConversationalistModel conversationalistModel)
        {
            var messageRecordVM =
                new MessageRecordViewModel(messageSqliteRecord.Hash, messageSqliteRecord.RecordDateTime);

            messageRecordVM.IsFromUser = messageContentModel.IsFromUser;
            messageRecordVM.MessageText = messageContentModel.MessageText;
            messageRecordVM.Conversationalist = ConvertToConversationalistViewModelFrom(conversationalistModel);

            return messageRecordVM;
        }

        public static ConversationalistViewModel ConvertToConversationalistViewModelFrom(
            SQLiteConversationalistModel conversationalistSQLiteModel)
        {
            var convVM = new ConversationalistViewModel();

            convVM.Id = conversationalistSQLiteModel.Id;
            convVM.Hash = conversationalistSQLiteModel.Hash;
            convVM.Name = conversationalistSQLiteModel.Name;
            convVM.Created = conversationalistSQLiteModel.Created;
            convVM.LastEdited = conversationalistSQLiteModel.LastEdited;

            return convVM;
        }
    }
}