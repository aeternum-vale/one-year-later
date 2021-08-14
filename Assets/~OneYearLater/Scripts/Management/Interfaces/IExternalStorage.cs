using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IExternalStorage
	{
		EExternalStorageKey Key { get; }
		string Name { get; }

		UniTask Authenticate();
		UniTask Sync();

	}
}
