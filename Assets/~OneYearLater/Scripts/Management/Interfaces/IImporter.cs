using Cysharp.Threading.Tasks;
using UniRx;

namespace OneYearLater.Management.Interfaces
{

	public struct ImportResult
	{
		public bool IsCanceled;
		public int ImportedRecordsCount;
		public int AbortedDuplicatesCount;
	}

	public interface IImporter
	{
		UniTask<ImportResult> ImportFromTextFile();
		ReactiveProperty<bool> IsImportingInProcess { get; }
		ReactiveProperty<float> ImportFromTextFileProgress { get; }
	}
}