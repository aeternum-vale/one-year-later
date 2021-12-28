using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IPopupManager
	{
		UniTask RunMessagePopupAsync(string messageText);
		UniTask RunMessagePopupAsync(string messageText, string okButtonText);

		UniTask<string> RunPromptPopupAsync(string messageText, string placeholderText, string okButtonText);
		UniTask<string> RunPromptPopupAsync(string messageText, string placeholderText);
		UniTask<string> RunPromptPopupAsync(string messageText);

		UniTask<bool> RunConfirmPopupAsync(string messageText);
		UniTask<bool> RunConfirmPopupAsync();

		bool IsAnyPopupActive { get; }
	}
}