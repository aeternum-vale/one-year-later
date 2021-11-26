using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Popups
{
	[RequireComponent(typeof(Popup))]
	public class ConfirmPopup : MonoBehaviour, ISpecificPopup
	{
		public Popup AbstractPopup { get; private set; }
		public EPopupKey Key => EPopupKey.Confirm;

		[SerializeField] private Button _yesButton;
		[SerializeField] private Button _noButton;

		public event EventHandler YesButtonClicked;
		public event EventHandler NoButtonClicked;

		public bool Answer { get; set; }


		private void Awake()
		{
			AbstractPopup = GetComponent<Popup>();

			_yesButton.onClick.AddListener(() => YesButtonClicked?.Invoke(this, EventArgs.Empty));
			_noButton.onClick.AddListener(() => NoButtonClicked?.Invoke(this, EventArgs.Empty));
		}

		public void Init(string messageText)
		{
			AbstractPopup.Init(messageText);
		}

		public async UniTask WaitForUserAnswerAsync()
		{
			Answer = false;
			bool isYesClicked = false;
			bool isNoClicked = false;

			EventHandler yesClickHandler = (s, a) => isYesClicked = true;
			EventHandler noClickHandler = (s, a) => isNoClicked = true;

			YesButtonClicked += yesClickHandler;
			NoButtonClicked += noClickHandler;

			await UniTask.WaitUntil(() => isYesClicked || isNoClicked);

			YesButtonClicked -= yesClickHandler;
			NoButtonClicked -= noClickHandler;

			Answer = isYesClicked;
		}
	}
}