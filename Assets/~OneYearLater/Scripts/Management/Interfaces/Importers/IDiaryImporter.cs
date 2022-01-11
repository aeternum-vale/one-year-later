using Cysharp.Threading.Tasks;
using UniRx;

namespace OneYearLater.Management.Interfaces.Importers
{
	public interface IDiaryImporter
	{
		UniTask<ImportResult> ImportFromTextFile();
		ReactiveProperty<bool> IsImportingInProcess { get; }
		ReactiveProperty<float> ImportFromTextFileProgress { get; }
	}
}