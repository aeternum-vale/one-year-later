using System;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IAppLocalStorage
	{
		UniTask UpdateExternalStorageStateAsync(EExternalStorageKey key, string state);
		UniTask UpdateExternalStorageSyncDateAsync(EExternalStorageKey key, DateTime syncDate);
		UniTask<ExternalStorageModel?> GetExternalStorageAsync(EExternalStorageKey key);
	}
}
