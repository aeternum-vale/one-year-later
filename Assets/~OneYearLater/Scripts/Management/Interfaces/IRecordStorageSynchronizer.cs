using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IRecordStorageSynchronizer
	{
		UniTask<bool> TrySyncLocalAndExternalRecordStorages(IExternalStorage externalStorage);
	}
}
