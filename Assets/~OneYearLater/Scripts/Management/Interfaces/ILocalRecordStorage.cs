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
		UniTask<BaseRecordViewModel> GetRecordAsync(string recordHash);

		UniTask InsertRecordAsync(BaseRecordViewModel record);
		UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records);

		UniTask DeleteRecordAsync(string recordHash);

		UniTask UpdateRecordAsync(BaseRecordViewModel record);

	}
}
