using Cysharp.Threading.Tasks;

namespace OneYearLater.UI.Popups
{
	public interface ISpecificPopup
	{
		EPopupKey Key { get; }
		Popup AbstractPopup { get; }

		UniTask WaitForUserAnswerAsync();
	}
}