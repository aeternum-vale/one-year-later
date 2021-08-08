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
		private Popup _popup;


		private void Awake()
		{
			_popup = GetComponent<Popup>();
		}

		public void Init(string messageText, string okButtonText, string inputFieldPlaceholder)
		{
			_popup.Init(messageText, okButtonText);

			_inputField.text = string.Empty;
			_inputField.placeholder.GetComponent<TMP_Text>().text = inputFieldPlaceholder;
		}

	}
}
