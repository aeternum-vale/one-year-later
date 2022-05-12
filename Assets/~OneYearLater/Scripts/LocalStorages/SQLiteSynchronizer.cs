using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using UnityEngine;
using Zenject;
using UniRx;
using static OneYearLater.LocalStorages.Constants;
using static OneYearLater.LocalStorages.Utils;
using static OneYearLater.LocalStorages.ModelsConverter;

namespace OneYearLater.LocalStorages
{
    public class SQLiteSynchronizer : IRecordStorageSynchronizer
    {
        [Inject] private HandledSQLiteRecordStorage _localRecordStorage;

        private IExternalStorage _externalStorage;

        private string _originalLocalDbPath;
        private string _externalDbPath;
        private string _externalDbLocalCopyNameWithExtension;
        private string _externalDbLocalCopyPath;
        private string _dbBackupNameWithExtension;
        private string _backupDbPath;

        private bool _isExternalDbFileExisted;
        private bool _isRollbackError = false;


        private SQLiteAsyncConnection _connectionToLocal;
        private SQLiteAsyncConnection _connectionToExternalCopy;

        public ReactiveProperty<bool> _isSyncInProcess = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsSyncInProcess => _isSyncInProcess;


        public SQLiteSynchronizer()
        {
            _originalLocalDbPath = GetDbPathOnDevice(RecordsDbNameWithExtension);

            _externalDbPath = $"/{RecordsDbNameWithExtension}";

            _externalDbLocalCopyNameWithExtension =
                $"{Path.GetFileNameWithoutExtension(RecordsDbNameWithExtension)}{ExternalDbLocalCopyPostfix}{Path.GetExtension(RecordsDbNameWithExtension)}";

            _externalDbLocalCopyPath = GetDbPathOnDevice(_externalDbLocalCopyNameWithExtension);

            _dbBackupNameWithExtension =
                $"{Path.GetFileNameWithoutExtension(RecordsDbNameWithExtension)}{BackupPostfix}{Path.GetExtension(RecordsDbNameWithExtension)}";

            _backupDbPath = GetDbPathOnDevice(_dbBackupNameWithExtension);
            
        }

        private void CreateAsyncConnectionToLocal()
        {
            _connectionToLocal = new SQLiteAsyncConnection(_originalLocalDbPath, SQLiteOpenFlags.ReadWrite);
        }

        private void CreateAsyncConnectionToExternalCopy()
        {
            _connectionToExternalCopy = new SQLiteAsyncConnection(_externalDbLocalCopyPath, SQLiteOpenFlags.ReadOnly);
        }
        
        

        private UniTask WaitUntilSyncIsNotInProcess()
        {
            if (_isSyncInProcess.Value)
                return UniTask.WaitUntil(() => !_isSyncInProcess.Value);
            return UniTask.CompletedTask;
        }

        public async UniTask<bool> TrySyncLocalAndExternalRecordStorages(IExternalStorage externalStorage)
        {
            await WaitUntilSyncIsNotInProcess();

            _isSyncInProcess.Value = true;
            _externalStorage = externalStorage;
            _isExternalDbFileExisted = await _externalStorage.IsFileExist(_externalDbPath);

            //await UniTask.Delay(TimeSpan.FromSeconds(10f), DelayType.Realtime);

            bool? isSuccess = null;

            if (await IsLocalDbMustAndCanBeRestored())
                isSuccess = await TryRestoreLocalDb();

            if (_isExternalDbFileExisted && isSuccess == null)
            {
                bool isBackupCreated = TryCreateBackup();
                if (!isBackupCreated)
                    isSuccess = false;
            }

            if (isSuccess == null)
                isSuccess = await TrySync();

            _isSyncInProcess.Value = false;
            Debug.Log($"<color=lightblue>{GetType().Name}:</color> Sync Process Is Over");
            return isSuccess.Value;
        }

        private async UniTask<bool> IsLocalDbMustAndCanBeRestored()
        {
            bool isLocalDbFileExisted = File.Exists(_originalLocalDbPath);
            bool isDatabaseValid = false;

            if (isLocalDbFileExisted)
                isDatabaseValid = await _localRecordStorage.IsDatabaseValid();

            return (!isLocalDbFileExisted || !isDatabaseValid) && _isExternalDbFileExisted;
        }

        private async UniTask<bool> TryRestoreLocalDb()
        {
            Debug.Log($"<color=lightblue>{GetType().Name}:</color> TryRestoreLocalDb");
            try
            {
                await _externalStorage.DownloadFile(_externalDbPath, _originalLocalDbPath);
                Debug.Log($"<color=lightblue>{GetType().Name}:</color> db restored!");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while restoring missing local db ({ex.Message})\n{ex.StackTrace}");
                return false;
            }
        }

        private bool TryCreateBackup()
        {
            try
            {
                File.Copy(_originalLocalDbPath, _backupDbPath, true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while creating backup ({ex.Message})\n{ex.StackTrace}");
                return false;
            }
        }

        private async UniTask<bool> TrySync()
        {
            try
            {
                await CloseAllConnections();
                CreateAsyncConnectionToLocal();
                await MarkAllRecordsAsNonLocal();

                if (_isExternalDbFileExisted)
                {
                    CreateAsyncConnectionToExternalCopy();
                    await _externalStorage.DownloadFile(_externalDbPath, _externalDbLocalCopyPath);
                    await ApplyToLocalDbChangesFromExternal();
                }

                await CloseAllConnections();
                await _externalStorage.UploadFile(_originalLocalDbPath, _externalDbPath);

                Debug.Log("External DB is Replaced by Local!");

                return true;
            }

            catch (Exception ex)
            {
                if (_isExternalDbFileExisted)
                {
                    Debug.LogError($"Error while sync, trying to rollback to backup. ({ex.Message})\n{ex.StackTrace}");
                    TryRollbackToBackup();
                }
                else
                    Debug.LogError($"Error while sync. ({ex.Message})\n{ex.StackTrace}");
                
                
                return false;
            }

            finally
            {
                await _localRecordStorage.Reconnect();
            }
        }

        private async UniTask MergeConversationalistsTables()
        {
            List<SQLiteConversationalistModel> allLocalConversationalists =
                await _connectionToLocal.Table<SQLiteConversationalistModel>().ToListAsync();

            List<SQLiteConversationalistModel> allExternalConversationalists =
                await _connectionToExternalCopy.Table<SQLiteConversationalistModel>().ToListAsync();

            Dictionary<string, SQLiteConversationalistModel> localConvHashDictionary =
                new Dictionary<string, SQLiteConversationalistModel>();

            allLocalConversationalists.ForEach(c => localConvHashDictionary[c.Hash] = c);

            List<SQLiteConversationalistModel> localConversationalistsToUpdate =
                new List<SQLiteConversationalistModel>();
            List<SQLiteConversationalistModel> localConversationalistsToInsert =
                new List<SQLiteConversationalistModel>();

            foreach (var externalConv in allExternalConversationalists)
            {
                var hash = externalConv.Hash;

                if (localConvHashDictionary.ContainsKey(hash))
                {
                    var localConv = localConvHashDictionary[hash];
                    bool isExternalOlder = externalConv.LastEdited < localConv.LastEdited;
                    if (isExternalOlder) continue;

                    localConv.Name = externalConv.Name;
                    localConv.LastEdited = externalConv.LastEdited;

                    localConversationalistsToUpdate.Add(localConv);
                }

                localConversationalistsToInsert.Add(externalConv);
            }

            await _connectionToLocal.UpdateAllAsync(localConversationalistsToUpdate);
            await _connectionToLocal.InsertAllAsync(localConversationalistsToInsert);
        }

        private async UniTask ApplyToLocalDbChangesFromExternal()
        {
            var query = _connectionToLocal.Table<SQLiteRecordModel>();
            List<SQLiteRecordModel> allLocalDbRecords = await query.ToListAsync();
            Dictionary<string, SQLiteRecordModel> localDbHashDictionary =
                new Dictionary<string, SQLiteRecordModel>();

            query = _connectionToExternalCopy.Table<SQLiteRecordModel>();
            List<SQLiteRecordModel> allExternalDbRecords = await query.ToListAsync();

            allLocalDbRecords.ForEach(r => localDbHashDictionary[r.Hash] = r);

            List<SQLiteRecordModel> localRecordsToUpdate = new List<SQLiteRecordModel>();
            List<SQLiteRecordModel> localRecordsToInsert = new List<SQLiteRecordModel>();

            await MergeConversationalistsTables();

            foreach (SQLiteRecordModel externalRecord in allExternalDbRecords)
            {
                var hash = externalRecord.Hash;
                if (localDbHashDictionary.ContainsKey(hash))
                {
                    SQLiteRecordModel localRecord = localDbHashDictionary[hash];

                    bool isExternalOlder = externalRecord.LastEdited < localRecord.LastEdited;

                    if (isExternalOlder) continue;

                    localRecord.LastEdited = externalRecord.LastEdited;
                    localRecord.RecordDateTime = externalRecord.RecordDateTime;
                    localRecord.IsDeleted = externalRecord.IsDeleted;

                    await ApplyToLocalDbRecordContentsChangesFromExternal(localRecord);
                    localRecordsToUpdate.Add(localRecord);

                    continue;
                }

                localRecordsToInsert.Add(externalRecord);
                await InsertToLocalDbRecordContentsFromExternal(externalRecord);
            }

            await UniTask.WhenAll(
                _connectionToLocal.UpdateAllAsync(localRecordsToUpdate),
                _connectionToLocal.InsertAllAsync(localRecordsToInsert)
            );
        }

        private async UniTask ApplyToLocalDbRecordContentsChangesFromExternal(SQLiteRecordModel localRecord)
        {
            var hash = localRecord.Hash;
            switch ((ERecordType) localRecord.Type)
            {
                case ERecordType.Diary:
                    var diaryLocalContent = await _connectionToLocal.Table<SQLiteDiaryContentModel>()
                        .Where(dc => dc.Hash == hash)
                        .FirstAsync();

                    var diaryExternalContent = await _connectionToExternalCopy.Table<SQLiteDiaryContentModel>()
                        .Where(dc => dc.Hash == hash)
                        .FirstAsync();

                    if (!diaryLocalContent.Text.Equals(diaryExternalContent.Text))
                    {
                        diaryLocalContent.Text = diaryExternalContent.Text;
                        await _connectionToLocal.UpdateAsync(diaryLocalContent);
                    }

                    break;

                case ERecordType.Message:
                    var messageLocalContent = await _connectionToLocal.Table<SQLiteMessageContentModel>()
                        .Where(mc => mc.Hash == hash)
                        .FirstAsync();

                    var messageExternalContent = await _connectionToExternalCopy
                        .Table<SQLiteMessageContentModel>()
                        .Where(mc => mc.Hash == hash)
                        .FirstAsync();

                    if (!messageLocalContent.MessageText.Equals(messageExternalContent.MessageText))
                    {
                        messageLocalContent.MessageText = messageExternalContent.MessageText;
                        await _connectionToLocal.UpdateAsync(messageLocalContent);
                    }

                    break;

                default: throw new Exception("unknown record type for sync");
            }
        }

        private async UniTask InsertToLocalDbRecordContentsFromExternal(SQLiteRecordModel externalRecord)
        {
            var hash = externalRecord.Hash;

            switch ((ERecordType) externalRecord.Type)
            {
                case ERecordType.Diary:
                    var externalDiaryContent = await _connectionToExternalCopy.Table<SQLiteDiaryContentModel>()
                        .Where(dc => dc.Hash == hash)
                        .FirstAsync();

                    await _connectionToLocal.InsertAsync(externalDiaryContent);
                    break;

                case ERecordType.Message:
                    var externalMessageContent = await _connectionToExternalCopy.Table<SQLiteMessageContentModel>()
                        .Where(mc => mc.Hash == hash)
                        .FirstAsync();

                    var externalConversationalist = await _connectionToExternalCopy
                        .Table<SQLiteConversationalistModel>()
                        .Where(c => c.Id == externalMessageContent.ConversationalistId)
                        .FirstAsync();

                    var localConversationalist = await _connectionToLocal.Table<SQLiteConversationalistModel>()
                        .Where(c => c.Hash == externalConversationalist.Hash).FirstAsync();

                    externalMessageContent.ConversationalistId = localConversationalist.Id;

                    await _connectionToLocal.InsertAsync(externalMessageContent);
                    break;
                default: throw new Exception("unknown record type for sync");
            }
        }

        private async UniTask MarkAllRecordsAsNonLocal()
        {
            await _connectionToLocal
                .ExecuteAsync(
                    $"UPDATE {nameof(SQLiteRecordModel)} SET {nameof(SQLiteRecordModel.IsLocal)} = 0"
                );
        }

        private async UniTask CloseAllConnections()
        {
            await _localRecordStorage.CloseAllConnections();
            
            if (_connectionToLocal != null)
                await _connectionToLocal.CloseAsync();

            if (_connectionToExternalCopy != null)
                await _connectionToExternalCopy.CloseAsync();
        }

        private bool TryRollbackToBackup()
        {
            try
            {
                RollbackToBackup();
                Debug.Log("successful rollback!");
                return true;
            }

            catch (Exception ex)
            {
                Debug.LogError($"Error while rolling back ({ex.Message})\n{ex.StackTrace}");
                _isRollbackError = true;
                return false;
            }
        }

        private void RollbackToBackup()
        {
            File.Copy(_backupDbPath, _originalLocalDbPath, true);
        }
    }
}