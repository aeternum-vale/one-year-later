using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using OneYearLater.UI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Popups
{
	[RequireComponent(typeof(CanvasGroupFader))]

	public class Popup : MonoBehaviour, IAsyncFadable
	{
		public event EventHandler OkButtonClicked;

		[SerializeField] private TMP_Text _message;
		[SerializeField] private Button _okButton;
		[SerializeField] [ReadOnly] private TMP_Text _okButtonTextComponent;
		[SerializeField] [ReadOnly] private CanvasGroupFader _canvasGroupFader;


		#region Unity Callbacks


		private void Awake()
		{
			PopulateFields();

			_canvasGroupFader.FadeDuration = Constants.PopupFadeDuration;

			_okButton.onClick.AddListener(OnOkButtonClick);
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			PopulateFields();
		}
#endif
		#endregion

		private void PopulateFields()
		{
			if (_canvasGroupFader == null)
				_canvasGroupFader = GetComponent<CanvasGroupFader>();

			if (_okButtonTextComponent == null)
				_okButtonTextComponent = _okButton.GetComponentInChildren<TMP_Text>();
		}

		public void Init(string messageText, string okButtonText)
		{
			if (string.IsNullOrEmpty(messageText))
				throw new Exception("Popup message text can't be null or empty");

			_message.text = messageText;

			if (!string.IsNullOrEmpty(okButtonText))
				_okButtonTextComponent.text = okButtonText;
		}

		private void OnOkButtonClick()
		{
			OkButtonClicked?.Invoke(this, EventArgs.Empty);
		}

		public UniTask FadeAsync() => _canvasGroupFader.FadeAsync();
		public UniTask UnfadeAsync() => _canvasGroupFader.UnfadeAsync();
		public UniTask FadeAsync(CancellationToken token) => _canvasGroupFader.FadeAsync(token);
		public UniTask UnfadeAsync(CancellationToken token) => _canvasGroupFader.UnfadeAsync(token);

	}
}
