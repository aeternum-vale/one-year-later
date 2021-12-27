using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management
{
	public class LocalStorageWithExceptionHandling : ILocalRecordStorage
	{
		[Inject] private ILocalRecordStorage _localRecordStorage;
		[Inject] private IPopupManager _popupManager;

		public UniTask DeleteRecordAsync(int recordId)
		{
			return HandleAsyncOperation(_localRecordStorage.DeleteRecordAsync(recordId));
		}

		public UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
		{
			return HandleAsyncOperation(_localRecordStorage.GetAllDayRecordsAsync(date));
		}

		public UniTask<BaseRecordViewModel> GetRecordAsync(int recordId)
		{
			return HandleAsyncOperation(_localRecordStorage.GetRecordAsync(recordId));
		}

		public UniTask InsertRecordAsync(BaseRecordViewModel record)
		{
			return HandleAsyncOperation(_localRecordStorage.InsertRecordAsync(record));
		}

		public UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			return HandleAsyncOperation(_localRecordStorage.InsertRecordsAsync(records));
		}

		public UniTask UpdateRecordAsync(BaseRecordViewModel record)
		{
			return HandleAsyncOperation(_localRecordStorage.UpdateRecordAsync(record));
		}

		private async UniTask HandleAsyncOperation(UniTask operation)
		{
			try
			{
				await operation;
			}
			catch (Exception ex)
			{
				HandleException(ex).Forget();
				throw ex;
			}
		}

		private async UniTask<T> HandleAsyncOperation<T>(UniTask<T> operation)
		{
			try
			{
				T result = await operation;
				return result;
			}
			catch (Exception ex)
			{
				HandleException(ex).Forget();
				throw ex;
			}
		}

		private async UniTask HandleException(Exception ex)
		{
			Debug.LogError(ex.Message);
			await _popupManager.RunMessagePopupAsync("Couldn't connect to record storage... Try to synchronize with some of the external storages or relaunch the app.");
		}
	}
}