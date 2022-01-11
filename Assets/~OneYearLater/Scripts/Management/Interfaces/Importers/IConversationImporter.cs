using Cysharp.Threading.Tasks;
using UniRx;

namespace OneYearLater.Management.Interfaces.Importers
{
	public interface IConversationImporter
	{
		UniTask<ImportResult> ImportFromTextFile();
		ReactiveProperty<bool> IsImportingInProcess { get; }
		ReactiveProperty<float> ImportFromTextFileProgress { get; }
	}
}