using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Popups
{
	[RequireComponent(typeof(Popup))]
	public class PromptPopup : MonoBehaviour, ISpecificPopup
	{
		public EPopupKey Key => EPopupKey.Prompt;
		public Popup AbstractPopup { get; private set; }
		public string InputFieldText => _inputField.text;

		[SerializeField] private TMP_InputField _inputField;


		private void Awake()
		{
			AbstractPopup = GetComponent<Popup>();
		}

		public void Init(string messageText, string okButtonText, string inputFieldPlaceholder)
		{
			AbstractPopup.Init(messageText, okButtonText);

			_inputField.text = string.Empty;
			_inputField.placeholder.GetComponent<TMP_Text>().text = inputFieldPlaceholder;
		}

		public UniTask WaitForUserAnswerAsync() => AbstractPopup.WaitForUserAnswerAsync();
	}
}
