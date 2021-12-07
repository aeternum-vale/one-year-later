using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IAppLocalStorage
	{
		UniTask UpdateExternalStorageStateAsync(EExternalStorageKey key, string state);
		UniTask UpdateExternalStorageSyncDateAsync(EExternalStorageKey key, DateTime syncDate);
		UniTask<ExternalStorageViewModel?> GetExternalStorageAsync(EExternalStorageKey key);
	}
}
