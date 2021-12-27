using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IPopupManager
	{
		UniTask RunMessagePopupAsync(string messageText);
		UniTask RunMessagePopupAsync(string messageText, string okButtonText);
		UniTask<string> RunPromptPopupAsync(string messageText, string okButtonText, string placeholderText);
		UniTask<bool> RunConfirmPopupAsync(string messageText);

		bool IsAnyPopupActive { get; }
	}
}