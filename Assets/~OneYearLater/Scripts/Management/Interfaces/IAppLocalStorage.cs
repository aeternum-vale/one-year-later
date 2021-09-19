using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IAppLocalStorage
	{
		UniTask SaveExternalStorageStateAsync(ExternalStorageModel state);
		UniTask<string> GetExternalStorageStateAsync(EExternalStorageKey key);
	}
}
