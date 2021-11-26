using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OneYearLater.UI.Popups
{
	[RequireComponent(typeof(Popup))]
	public class MessagePopup : MonoBehaviour, ISpecificPopup
	{
		public EPopupKey Key => EPopupKey.Message;
		public Popup AbstractPopup { get; private set; }

		private void Awake()
		{
			AbstractPopup = GetComponent<Popup>();
		}

		public UniTask WaitForUserAnswerAsync() => AbstractPopup.WaitForUserAnswerAsync();

		public void Init(string messageText, string okButtonText = null) => 
			AbstractPopup.Init(messageText, okButtonText);
	}
}