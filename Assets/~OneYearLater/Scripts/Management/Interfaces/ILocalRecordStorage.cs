using OneYearLater.Management.ViewModels;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface ILocalRecordStorage
	{
		UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date);
		UniTask<BaseRecordViewModel> GetRecordAsync(int recordId);

		UniTask InsertRecordAsync(BaseRecordViewModel record);
		UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records);

		UniTask DeleteRecordAsync(int recordId);

		UniTask UpdateRecordAsync(BaseRecordViewModel record);
	}
}
