using System;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Popups
{
	[RequireComponent(typeof(Popup))]
	public class ConfirmPopup : MonoBehaviour
	{
		[SerializeField] private Button _yesButton;
		[SerializeField] private Button _noButton;

		private Popup _abstractPopup;
		public Popup AbstractPopup { get => _abstractPopup; }

		public event EventHandler YesButtonClicked;
		public event EventHandler NoButtonClicked;


		private void Awake()
		{
			_abstractPopup = GetComponent<Popup>();

			_yesButton.onClick.AddListener(() => YesButtonClicked?.Invoke(this, EventArgs.Empty));
			_noButton.onClick.AddListener(() => NoButtonClicked?.Invoke(this, EventArgs.Empty));
		}

		public void Init(string messageText)
		{
			_abstractPopup.Init(messageText);
		}
	}
}