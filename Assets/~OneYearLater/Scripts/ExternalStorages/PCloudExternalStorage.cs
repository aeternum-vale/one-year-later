using Cysharp.Threading.Tasks;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;


namespace ExternalStorages
{
	public class PCloudExternalStorage : IExternalStorage
	{
		public EExternalStorageKey Key => EExternalStorageKey.PCloud;
		public string Name => "pCloud";


		public UniTask DownloadFile(string externalStoragePath, string localStoragePath)
		{
			throw new System.NotImplementedException();
		}

		public UniTask<bool> IsFileExist(string path)
		{
			throw new System.NotImplementedException();
		}

		public UniTask<bool> Connect(string code)
		{
			throw new System.NotImplementedException();
		}

		public void RequestAccessCode()
		{
			throw new System.NotImplementedException();
		}

		public UniTask Synchronize()
		{
			throw new System.NotImplementedException();
		}

		public UniTask UploadFile(string localStoragePath, string externalStoragePath)
		{
			throw new System.NotImplementedException();
		}
	}
}
