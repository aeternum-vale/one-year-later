using Cysharp.Threading.Tasks;
using UniRx;

namespace OneYearLater.Management.Interfaces
{
	public interface IExternalStorage
	{
		EExternalStorageKey Key { get; }
		string Name { get; }
		ReactiveProperty<string> PersistentState { get; }
		bool IsWaitingForAccessCode { get; }

		void Init(string state);
		void RequestAccessCode();
		UniTask<bool> ConnectWithAccessCode(string accessCode);
		UniTask<bool> IsConnected();
		UniTask Disconnect();
		UniTask<bool> IsFileExist(string path);
		UniTask DownloadFile(string externalStoragePath, string localStoragePath);
		UniTask UploadFile(string localStoragePath, string externalStoragePath);
	}
}
