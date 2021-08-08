using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static Utilities.Extensions;
namespace OneYearLater.UI.Popups
{
	[RequireComponent(typeof(CanvasGroupFader))]

	public class Popup : MonoBehaviour
	{
		public event EventHandler OkButtonClicked;

		[SerializeField] private TMP_Text _message;
		[SerializeField] private Button _okButton;

		[Header("Animation")]

		[SerializeField] Ease _easeShow = Ease.InBounce;
		[SerializeField] Ease _easeHide = Ease.OutBounce;
		[SerializeField] float _fromScale = 0.7f;


		private CanvasGroupFader _canvasGroupFader;
		private TMP_Text _okButtonTextComponent;


		private void Awake()
		{
			_okButtonTextComponent = _okButton.GetComponentInChildren<TMP_Text>();
			_okButton.onClick.AddListener(OnOkButtonClick);

			_canvasGroupFader = GetComponent<CanvasGroupFader>();
			_canvasGroupFader.OwnFadeDuration = Constants.PopupAppearDuration;
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

		public UniTask PlayShowAnimation()
		{
			transform.localScale = new Vector3(_fromScale, _fromScale, 1);
			return transform.DOScale(Vector3.one, Constants.PopupAppearDuration)
				.SetEase(_easeShow)
				.ToUniTask();
		}

		public UniTask PlayHideAnimation()
		{
			transform.localScale = Vector3.one;
			return transform.DOScale(new Vector3(_fromScale, _fromScale, 1), Constants.PopupAppearDuration)
				.SetEase(_easeHide)
				.ToUniTask();
		}

	}
}
