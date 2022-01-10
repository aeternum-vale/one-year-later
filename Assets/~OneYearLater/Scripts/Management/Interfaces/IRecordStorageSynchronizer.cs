using UniRx;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IRecordStorageSynchronizer
	{
		ReactiveProperty<bool> IsSyncInProcess { get; }
		UniTask<bool> TrySyncLocalAndExternalRecordStorages(IExternalStorage externalStorage);
	}
}
