using OneYearLater.Management.ViewModels;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface ILocalRecordStorage
	{
		UniTask SaveRecordsAsync(IEnumerable<BaseRecordViewModel> records);
		UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date);
		UniTask<BaseRecordViewModel> GetRecordAsync(int recordId);
		UniTask<bool> SyncLocalAndExternalRecordStoragesAsync(IExternalStorage externalStorage);
	}
}
