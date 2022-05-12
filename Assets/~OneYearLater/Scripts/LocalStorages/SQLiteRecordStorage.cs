using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using Zenject;
using static OneYearLater.LocalStorages.ModelsConverter;

namespace OneYearLater.LocalStorages
{
    public class SQLiteRecordStorage : ILocalRecordStorage
    {
        private RecordStorageConnector _recordStorageConnector;
        private SQLiteAsyncConnection _readWriteAsyncConnection;
        private SQLiteAsyncConnection _readOnlyAsyncConnection;

        public SQLiteRecordStorage()
        {
            _recordStorageConnector = new RecordStorageConnector();
        }

        public SQLiteRecordStorage(string dbNameWithExtension)
        {
            _recordStorageConnector = new RecordStorageConnector(dbNameWithExtension);
        }

        public async UniTask<EInitResult> Init()
        {
            var result = await _recordStorageConnector.InitDatabase();
            await Reconnect();
            return result;
        }

        public async UniTask Reconnect()
        {
            _readWriteAsyncConnection = await _recordStorageConnector.GetReadWriteAsyncConnection();
            _readOnlyAsyncConnection = await _recordStorageConnector.GetReadOnlyAsyncConnection();
        }

        public UniTask<bool> IsDatabaseValid() => _recordStorageConnector.IsDatabaseValid();
        public UniTask CloseAllConnections() => _recordStorageConnector.CloseAllConnections();

        public async UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
        {
            DateTime dayStartInc = date.Date;
            DateTime dayEndExc = date.Date.AddDays(1);

            var query =
                _readOnlyAsyncConnection.Table<SQLiteRecordModel>()
                    .Where(r => r.RecordDateTime >= dayStartInc && (r.RecordDateTime < dayEndExc) && (!r.IsDeleted))
                    .OrderBy(r => r.RecordDateTime);

            List<SQLiteRecordModel> incompleteRecords = await query.ToListAsync();
            return await UniTask.WhenAll(incompleteRecords.Select(FulfilRecord));
        }
        

        public async UniTask<BaseRecordViewModel> GetRecordAsync(string recordHash)
        {
            var sqliteRecord =
                await _readWriteAsyncConnection.Table<SQLiteRecordModel>()
                    .Where(r => (r.Hash == recordHash) && (!r.IsDeleted))
                    .FirstAsync();

            return await FulfilRecord(sqliteRecord);
        }

        public async UniTask InsertRecordAsync(BaseRecordViewModel recordVM)
        {
            SQLiteRecordModel recordModel = ConvertToSQLiteRecordModelFrom(recordVM);

            var existedCount = await _readWriteAsyncConnection
                .Table<SQLiteRecordModel>()
                .CountAsync(r => r.Hash.Equals(recordModel.Hash) && r.RecordDateTime == recordModel.RecordDateTime);
            if (existedCount > 0)
                throw new RecordDuplicateException();

            await _readWriteAsyncConnection.InsertAsync(recordModel);

            recordVM.Hash = recordModel.Hash;

            switch (recordVM.Type)
            {
                case ERecordType.Diary:
                    await InsertDiaryContent((DiaryRecordViewModel) recordVM);
                    break;
                case ERecordType.Message:
                    var messageVM = (MessageRecordViewModel) recordVM;
                    await InsertMessageContent(messageVM);
                    break;
                default: throw new Exception("unknown record type");
            }
        }

        public UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
        {
            return UniTask.WhenAll(records.Select(r => InsertRecordAsync(r)));
        }

        public async UniTask UpdateRecordAsync(BaseRecordViewModel recordVM)
        {
            var recordModel = await RetrieveSQLiteRecordModelBy(recordVM.Hash);
            recordModel.LastEdited = DateTime.Now;
            recordModel.RecordDateTime = recordVM.RecordDateTime;

            switch (recordVM.Type)
            {
                case ERecordType.Diary:

                    var diaryVM = (DiaryRecordViewModel) recordVM;
                    var diaryContentModel = await RetrieveSQLiteDiaryContentModelBy(diaryVM.Hash);
                    diaryContentModel.Text = diaryVM.Text;
                    await _readWriteAsyncConnection.UpdateAsync(diaryContentModel);
                    break;

                case ERecordType.Message:

                    var messageVM = (MessageRecordViewModel) recordVM;
                    var messageContentModel = await RetrieveSQLiteMessageContentModelBy(messageVM.Hash);
                    messageContentModel.MessageText = messageVM.MessageText;
                    await _readWriteAsyncConnection.UpdateAsync(messageContentModel);
                    break;
            }

            await _readWriteAsyncConnection.UpdateAsync(recordModel);
        }

        public async UniTask DeleteRecordAsync(string recordHash)
        {
            SQLiteRecordModel recordModel = await RetrieveSQLiteRecordModelBy(recordHash);

            UniTask recordDeletingTask = UniTask.FromException(new Exception("invalid deleting task"));
            UniTask contentDeletingTask = UniTask.FromException(new Exception("invalid deleting task"));


            if (recordModel.IsLocal)
                recordDeletingTask = _readWriteAsyncConnection.DeleteAsync(recordModel);
            else
            {
                recordModel.IsDeleted = true;
                recordModel.LastEdited = DateTime.Now;
                recordDeletingTask = _readWriteAsyncConnection.UpdateAsync(recordModel);
            }

            switch ((ERecordType) recordModel.Type)
            {
                case ERecordType.Diary:
                    contentDeletingTask = _readWriteAsyncConnection.Table<SQLiteDiaryContentModel>()
                        .DeleteAsync(dc => dc.Hash == recordModel.Hash);
                    break;
                case ERecordType.Message:
                    contentDeletingTask = _readWriteAsyncConnection.Table<SQLiteMessageContentModel>()
                        .DeleteAsync(mc => mc.Hash == recordModel.Hash);
                    break;
            }

            await UniTask.WhenAll(recordDeletingTask, contentDeletingTask);
        }

        private async UniTask<BaseRecordViewModel> FulfilRecord(SQLiteRecordModel incompleteRecord)
        {
            switch ((ERecordType) incompleteRecord.Type)
            {
                case ERecordType.Diary:
                    var diaryContentModel = await _readOnlyAsyncConnection.Table<SQLiteDiaryContentModel>()
                        .FirstAsync(r => r.Hash == incompleteRecord.Hash);
                    return ConvertToRecordViewModelFrom(incompleteRecord, diaryContentModel);
                case ERecordType.Message:
                    var messageContentModel = await _readOnlyAsyncConnection.Table<SQLiteMessageContentModel>()
                        .FirstAsync(r => r.Hash == incompleteRecord.Hash);
                    var conversationalist = (await _readOnlyAsyncConnection.Table<SQLiteConversationalistModel>()
                        .FirstAsync(c => c.Id == messageContentModel.ConversationalistId));
                    return ConvertToRecordViewModelFrom(incompleteRecord, messageContentModel, conversationalist);
            }

            throw new Exception("invalid record type");
        }

        private async UniTask<SQLiteConversationalistModel> InsertConversationalistAsync(
            ConversationalistViewModel convVM)
        {
            DateTime now = DateTime.Now;

            StringBuilder stringForHashingBuilder = new StringBuilder();

            stringForHashingBuilder.Append(now.ToString(CultureInfo.InvariantCulture));
            stringForHashingBuilder.Append(convVM.Name);

            var conversationalistModel = new SQLiteConversationalistModel()
            {
                Created = now,
                LastEdited = now,
                Hash = Utilities.Utils.GetSHA256Hash(stringForHashingBuilder.ToString()),
            };

            await _readWriteAsyncConnection.InsertAsync(conversationalistModel);
            return conversationalistModel;
        }


        private async UniTask<SQLiteMessageContentModel> InsertMessageContent(MessageRecordViewModel messageVM)
        {
            var messageContentModel = new SQLiteMessageContentModel()
            {
                Hash = messageVM.Hash,
                MessageText = messageVM.MessageText,
                IsFromUser = messageVM.IsFromUser,
                ConversationalistId = messageVM.Conversationalist.Id
            };

            await _readWriteAsyncConnection.InsertAsync(messageContentModel);
            return messageContentModel;
        }

        private async UniTask<SQLiteDiaryContentModel> InsertDiaryContent(DiaryRecordViewModel diaryVM)
        {
            var diaryContentModel = new SQLiteDiaryContentModel() {Hash = diaryVM.Hash, Text = diaryVM.Text};
            await _readWriteAsyncConnection.InsertAsync(diaryContentModel);
            return diaryContentModel;
        }

        private UniTask<SQLiteRecordModel> RetrieveSQLiteRecordModelBy(string hash) =>
            _readWriteAsyncConnection.Table<SQLiteRecordModel>().Where(r => r.Hash == hash).FirstAsync();

        private UniTask<SQLiteDiaryContentModel> RetrieveSQLiteDiaryContentModelBy(string hash) =>
            _readWriteAsyncConnection.Table<SQLiteDiaryContentModel>().Where(dc => dc.Hash == hash).FirstAsync();

        private UniTask<SQLiteMessageContentModel> RetrieveSQLiteMessageContentModelBy(string hash) =>
            _readWriteAsyncConnection.Table<SQLiteMessageContentModel>().Where(mc => mc.Hash == hash).FirstAsync();
    }
}