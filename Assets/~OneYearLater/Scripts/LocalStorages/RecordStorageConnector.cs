using System;
using System.IO;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management.Interfaces;
using SQLite;
using UnityEngine;
using static OneYearLater.LocalStorages.Constants;
using static OneYearLater.LocalStorages.Utils;


namespace OneYearLater.LocalStorages
{
    public class RecordStorageConnector
    {
        private SQLiteAsyncConnection _readWriteAsyncConnection;
        private SQLiteAsyncConnection _readOnlyAsyncConnection;
        private SQLiteConnection _readWriteConnection;
        private SQLiteConnection _readOnlyConnection;
        private bool _isDbInitiated = false;

        private readonly string _dbNameWithExtension;

        public RecordStorageConnector()
        {
            _dbNameWithExtension = RecordsDbNameWithExtension;
        }

        public RecordStorageConnector(string dbNameWithExtension)
        {
            _dbNameWithExtension = dbNameWithExtension;
        }
        
		public async UniTask<EInitResult> InitDatabase()
		{
            EInitResult? result = null;

            if (_isDbInitiated)
                throw new Exception("db has already been initiated");

            string dbPath = GetDbPathOnDevice(_dbNameWithExtension);

            if (File.Exists(dbPath))
            {
                bool isDatabaseValid = await IsDatabaseValid();

                if (!isDatabaseValid)
                {
                    File.Delete(dbPath);
                    result = EInitResult.InvalidDatabase;
                }
            }
            else
                result = EInitResult.NoDatabase;

            var tempConnection = new SQLiteAsyncConnection(dbPath);
            await tempConnection.CreateTableAsync<SQLiteRecordModel>();
            await tempConnection.CreateTableAsync<SQLiteDiaryContentModel>();
            await tempConnection.CreateTableAsync<SQLiteMessageContentModel>();
            await tempConnection.CreateTableAsync<SQLiteConversationalistModel>();
            await tempConnection.CreateTableAsync<SQLiteRecordModel>();
            await tempConnection.CloseAsync();

            _isDbInitiated = true;

            if (result == null)
                result = EInitResult.ValidDatabase;

            return result.Value;
        }

        public async UniTask<SQLiteAsyncConnection> GetReadOnlyAsyncConnection()
        {
            await WaitUntilDbInitiated();

            InitializeAsyncReadOnlyConnection();

            return _readOnlyAsyncConnection;
        }

        public async UniTask<SQLiteAsyncConnection> GetReadWriteAsyncConnection()
        {
            await WaitUntilDbInitiated();

            InitializeAsyncReadWriteConnection();

            return _readWriteAsyncConnection;
        }
        
        public SQLiteConnection GetReadOnlyConnection()
        {
            if (!_isDbInitiated)
                throw new Exception("db is not initialized");
            
            InitializeReadOnlyConnection();

            return _readOnlyConnection;
        }

        public SQLiteConnection GetReadWriteConnection()
        {
            if (!_isDbInitiated)
                throw new Exception("db is not initialized");

            InitializeReadWriteConnection();

            return _readWriteConnection;
        }

        public async UniTask CloseAllConnections()
        {
            if (_readWriteAsyncConnection != null)
                await _readWriteAsyncConnection.CloseAsync();

            _readWriteAsyncConnection = null;

            if (_readOnlyAsyncConnection != null)
                await _readOnlyAsyncConnection.CloseAsync();

            _readOnlyAsyncConnection = null;

            _readWriteConnection?.Close();
            _readWriteConnection = null;

            _readOnlyConnection?.Close();
            _readOnlyConnection = null;
        }

        public async UniTask<bool> IsDatabaseValid()
        {
            SQLiteAsyncConnection conn = null;
            try
            {
                conn = OpenNewAsyncReadOnlyConnection();
                await conn.Table<SQLiteRecordModel>().FirstOrDefaultAsync();
                await conn.CloseAsync();
                return true;
            }

            catch (Exception)
            {
                return false;
            }

            finally
            {
                if (conn != null)
                    await conn.CloseAsync();
            }
        }

        private UniTask WaitUntilDbInitiated()
        {
            if (_isDbInitiated)
                return UniTask.CompletedTask;

            return UniTask.WaitUntil(() => _isDbInitiated);
        }

        
        private SQLiteAsyncConnection OpenNewAsyncReadWriteConnection() =>
            OpenNewAsyncConnection(SQLiteOpenFlags.ReadWrite);

        private SQLiteAsyncConnection OpenNewAsyncReadOnlyConnection() =>
            OpenNewAsyncConnection(SQLiteOpenFlags.ReadOnly);

        private SQLiteAsyncConnection OpenNewAsyncConnection(SQLiteOpenFlags flag) =>
            new SQLiteAsyncConnection(GetDbPathOnDevice(_dbNameWithExtension), flag);

        private void InitializeAsyncReadWriteConnection() =>
            _readWriteAsyncConnection ??= OpenNewAsyncReadWriteConnection();
        
        private void InitializeAsyncReadOnlyConnection() =>
            _readOnlyAsyncConnection ??= OpenNewAsyncReadOnlyConnection();
        
        private SQLiteConnection OpenNewReadWriteConnection() =>
            OpenNewConnection(SQLiteOpenFlags.ReadWrite);

        private SQLiteConnection OpenNewReadOnlyConnection() =>
            OpenNewConnection(SQLiteOpenFlags.ReadOnly);

        private SQLiteConnection OpenNewConnection(SQLiteOpenFlags flag) =>
            new SQLiteConnection(GetDbPathOnDevice(_dbNameWithExtension), flag);

        private void InitializeReadWriteConnection() =>
            _readWriteConnection ??= OpenNewReadWriteConnection();
        
        private void InitializeReadOnlyConnection() =>
            _readOnlyConnection ??= OpenNewReadOnlyConnection();
    }
}