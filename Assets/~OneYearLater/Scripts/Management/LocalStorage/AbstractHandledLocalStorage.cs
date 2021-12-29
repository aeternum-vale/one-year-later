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

		public UniTask DeleteRecordAsync(int recordId) =>
			Handle(LocalRecordStorage.DeleteRecordAsync(recordId));
		public UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date) =>
			Handle(LocalRecordStorage.GetAllDayRecordsAsync(date));
		public UniTask<BaseRecordViewModel> GetRecordAsync(int recordId) =>
			Handle(LocalRecordStorage.GetRecordAsync(recordId));
		public UniTask InsertRecordAsync(BaseRecordViewModel record) =>
			Handle(LocalRecordStorage.InsertRecordAsync(record));
		public UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records) =>
			Handle(LocalRecordStorage.InsertRecordsAsync(records));
		public UniTask UpdateRecordAsync(BaseRecordViewModel record) =>
			Handle(LocalRecordStorage.UpdateRecordAsync(record));

		private UniTask Handle(UniTask operation)
		{
			UniTask<bool> genericOperation = operation.ContinueWith(() => true);
			return Handle(genericOperation);
		}

		protected abstract UniTask<T> Handle<T>(UniTask<T> operation);
	}
}