using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Popups
{
	[RequireComponent(typeof(Popup))]
	public class PromptPopup : MonoBehaviour
	{
		public string InputFieldText => _inputField.text;

		[SerializeField] private TMP_InputField _inputField;
		private Popup _abstractPopup;
		public Popup AbstractPopup { get => _abstractPopup; }


		private void Awake()
		{
			_abstractPopup = GetComponent<Popup>();
		}

		public void Init(string messageText, string okButtonText, string inputFieldPlaceholder)
		{
			_abstractPopup.Init(messageText, okButtonText);

			_inputField.text = string.Empty;
			_inputField.placeholder.GetComponent<TMP_Text>().text = inputFieldPlaceholder;
		}

	}
}
