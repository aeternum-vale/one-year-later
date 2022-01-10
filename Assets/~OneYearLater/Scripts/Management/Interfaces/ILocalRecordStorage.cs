using OneYearLater.Management.ViewModels;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public enum EInitResult { ValidDatabase = 1, NoDatabase, InvalidDatabase }
	public interface ILocalRecordStorage
	{
		UniTask<EInitResult> Init();
		UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date);
		UniTask<BaseRecordViewModel> GetRecordAsync(int recordId);

		UniTask InsertRecordAsync(BaseRecordViewModel record);
		UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records);

		UniTask DeleteRecordAsync(int recordId);

		UniTask UpdateRecordAsync(BaseRecordViewModel record);
	}
}
