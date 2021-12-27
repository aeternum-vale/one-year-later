using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public enum EInitResult { ValidDatabase = 1, NoDatabase, InvalidDatabase }
	public interface IRecordStorageConnector
	{
		UniTask<EInitResult> InitDatabase();
	}

}