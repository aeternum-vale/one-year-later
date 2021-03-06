using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.LocalStorage
{
	public abstract class AbstractHandledLocalStorage : ILocalRecordStorage
	{
		protected abstract ILocalRecordStorage LocalRecordStorage { get; }


		public UniTask<EInitResult> Init() => LocalRecordStorage.Init();

		public UniTask DeleteRecordAsync(string recordHash) =>
			Handle(LocalRecordStorage.DeleteRecordAsync(recordHash));
		public UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date) =>
			Handle(LocalRecordStorage.GetAllDayRecordsAsync(date));
		public UniTask<BaseRecordViewModel> GetRecordAsync(string recordHash) =>
			Handle(LocalRecordStorage.GetRecordAsync(recordHash));
		public UniTask InsertRecordAsync(BaseRecordViewModel record) =>
			Handle(LocalRecordStorage.InsertRecordAsync(record));
		public UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records) =>
			Handle(LocalRecordStorage.InsertRecordsAsync(records));
		public UniTask UpdateRecordAsync(BaseRecordViewModel record) =>
			Handle(LocalRecordStorage.UpdateRecordAsync(record));


		protected UniTask Handle(UniTask operation)
		{
			UniTask<bool> genericOperation = operation.ContinueWith(() => true);
			return Handle(genericOperation);
		}

		protected abstract UniTask<T> Handle<T>(UniTask<T> operation);
	}
}