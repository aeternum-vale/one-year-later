using Cysharp.Threading.Tasks;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;


namespace ExternalStorages
{


	public class PCloudExternalStorage : IExternalStorage
	{
		public EExternalStorageKey Key => EExternalStorageKey.PCloud;
		public string Name => "pCloud";

		public UniTask Authenticate()
		{
			throw new System.NotImplementedException();
		}

		public UniTask Sync()
		{
			throw new System.NotImplementedException();
		}
	}
}
