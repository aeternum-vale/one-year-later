using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.LocalStorages.Models;
using OneYearLater.Management;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using SQLite;
using Zenject;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

using static OneYearLater.LocalStorages.ModelsConverter;

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

			List<SQLiteRecordModel> incompleteRecords = await query.ToListAsync();
			List<BaseRecordViewModel> resultVMs = new List<BaseRecordViewModel>();
			return await UniTask.WhenAll(incompleteRecords.Select(ir => FullfilRecord(ir)));
		}

		public async UniTask<BaseRecordViewModel> GetRecordAsync(int recordId)
		{
			var sqliteRecord =
				await _connection.Table<SQLiteRecordModel>()
					.Where(r => (r.Id == recordId) && (!r.IsDeleted))
					.FirstAsync();

			return await FullfilRecord(sqliteRecord);
		}

		private async UniTask<BaseRecordViewModel> FullfilRecord(SQLiteRecordModel incompleteRecord)
		{
			var readOnlyConnection = await _recordStorageConnector.GetReadOnlyConnection();

			switch ((ERecordType)incompleteRecord.Type)
			{
				case ERecordType.Diary:
					var diaryContentModel = await readOnlyConnection.Table<SQLiteDiaryContentModel>().FirstAsync(r => r.Id == incompleteRecord.ContentId);
					return ConvertToRecordViewModelFrom(incompleteRecord, diaryContentModel);
				case ERecordType.Message:
					var messageContentModel = await readOnlyConnection.Table<SQLiteMessengeContentModel>().FirstAsync(r => r.Id == incompleteRecord.ContentId);
					var conversationalistName = (await readOnlyConnection.Table<SQLiteConversationalistModel>().FirstAsync(c => c.Id == messageContentModel.ConversationalistId)).Name;
					return ConvertToRecordViewModelFrom(incompleteRecord, messageContentModel, conversationalistName);
			}

			throw new Exception("invalid record type");
		}

		public async UniTask InsertRecordAsync(BaseRecordViewModel recordVM)
		{
			int contentId = 0;

			switch (recordVM.Type)
			{
				case ERecordType.Diary:
					var diaryContent = await CreateDiaryContent((DiaryRecordViewModel)recordVM);
					contentId = diaryContent.Id;
					break;
				case ERecordType.Message:
					var messageVM = (MessageRecordViewModel)recordVM;
					int conversationalistId = await GetConversationalistId(messageVM.ConversationalistName);
					var messageContent = await CreateMessageContent(messageVM, conversationalistId);
					contentId = messageContent.Id;
					break;
			}

			SQLiteRecordModel recordModel = ConvertToSQLiteRecordModelFrom(recordVM, contentId);

			var existedCount = await _connection.Table<SQLiteRecordModel>()
				.Where(r => r.Hash.Equals(recordModel.Hash) && r.RecordDateTime == recordModel.RecordDateTime)
				.CountAsync();
			if (existedCount > 0)
				throw new RecordDuplicateException();

			await _connection.InsertAsync(recordModel);
		}

		private async UniTask<int> GetConversationalistId(string conversationalistName)
		{
			var readOnlyConnection = await _recordStorageConnector.GetReadOnlyConnection();

			var existingConversationalist =
				await readOnlyConnection.Table<SQLiteConversationalistModel>().FirstOrDefaultAsync(c => c.Name == conversationalistName);

			if (existingConversationalist != null)
				return existingConversationalist.Id;


			var newConversationalist = new SQLiteConversationalistModel() { Name = conversationalistName };
			await _connection.InsertAsync(newConversationalist);
			return newConversationalist.Id;
		}

		private async UniTask<SQLiteMessengeContentModel> CreateMessageContent(MessageRecordViewModel messageVM, int conversationalistId)
		{
			var messageContentModel = new SQLiteMessengeContentModel()
			{
				MessageText = messageVM.MessageText,
				IsFromUser = messageVM.IsFromUser,
				ConversationalistId = conversationalistId
			};

			await _connection.InsertAsync(messageContentModel);
			return messageContentModel;
		}

		private async UniTask<SQLiteDiaryContentModel> CreateDiaryContent(DiaryRecordViewModel diaryVM)
		{
			var diaryContentModel = new SQLiteDiaryContentModel() { Text = diaryVM.Text };
			await _connection.InsertAsync(diaryContentModel);
			return diaryContentModel;
		}

		public UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			return UniTask.WhenAll(records.Select(r => InsertRecordAsync(r)));
		}

		public async UniTask UpdateRecordAsync(BaseRecordViewModel recordVM)
		{
			switch (recordVM.Type)
			{
				case ERecordType.Diary:

					var diaryVM = (DiaryRecordViewModel)recordVM;

					var diaryRecordModel = await RetrieveSQLiteRecordModelBy(diaryVM.Id);
					var diaryContentModel = await RetrieveSQLiteDiaryContentModelBy(diaryRecordModel.ContentId);

					diaryRecordModel.RecordDateTime = diaryVM.DateTime;
					diaryRecordModel.LastEdited = DateTime.Now;
					diaryContentModel.Text = diaryVM.Text;

					await _connection.UpdateAsync(diaryRecordModel);
					await _connection.UpdateAsync(diaryContentModel);

					break;

				default: throw new Exception("invalid record type");
			}
		}

		public async UniTask DeleteRecordAsync(int recordId)
		{
			var sqliteRecord = await RetrieveSQLiteRecordModelBy(recordId);

			if (sqliteRecord.IsLocal)
			{
				await _connection.DeleteAsync(sqliteRecord); //TODO and content too
			}
			else
			{
				sqliteRecord.IsDeleted = true;
				sqliteRecord.LastEdited = DateTime.Now;
				await _connection.UpdateAsync(sqliteRecord);
			}
		}

		private UniTask<SQLiteRecordModel> RetrieveSQLiteRecordModelBy(int id)
		{
			return _connection.Table<SQLiteRecordModel>().Where(r => r.Id == id).FirstAsync();
		}

		private UniTask<SQLiteDiaryContentModel> RetrieveSQLiteDiaryContentModelBy(int id)
		{
			return _connection.Table<SQLiteDiaryContentModel>().Where(dc => dc.Id == id).FirstAsync();
		}

	}
}
