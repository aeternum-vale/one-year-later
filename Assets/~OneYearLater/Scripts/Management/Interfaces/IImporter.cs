using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{

	public interface IImporter
	{
		UniTask ImportFromTextFile();
	}
}