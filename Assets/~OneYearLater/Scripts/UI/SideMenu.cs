using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using static Utilities.Extensions;

namespace OneYearLater.UI
{
	public class SideMenu : MonoBehaviour
	{

		public event EventHandler FeedButtonClick;
		public event EventHandler ExternalStoragesButtonClick;

		[SerializeField] RectTransform _panel;
		[SerializeField] CanvasGroupFader _background;

		[SerializeField] Button _feedButton;
		[SerializeField] Button _externalStoragesButton;


		[Header("Animation")]
		[SerializeField] Ease _easeOpen = Ease.InBounce;
		[SerializeField] Ease _easeClose = Ease.OutBounce;

		[SerializeField] float _duration = 1f;

		public bool IsOpened { get; private set; } = false;

		private CancellationTokenSource _openCloseAnimationCTS;

		private void Start()
		{
			_panel.anchoredPosition = new Vector2(-_panel.sizeDelta.x, 0f);

			_feedButton.onClick.AddListener(() => { FeedButtonClick?.Invoke(this, EventArgs.Empty); });

			_externalStoragesButton.onClick.AddListener(() => { ExternalStoragesButtonClick?.Invoke(this, EventArgs.Empty); });
		}

		[Button]
		public void Open()
		{
			if (!IsOpened)
			{
				_openCloseAnimationCTS?.Cancel();
				_openCloseAnimationCTS = new CancellationTokenSource();
				var token = _openCloseAnimationCTS.Token;

				_panel.gameObject.SetActive(true);


				PlayPanelOpenAnimation(token).Forget();
				_background.UnfadeAsync(token).Forget();

				IsOpened = true;
			}
		}

		[Button]
		public void Close()
		{
			if (IsOpened)
			{
				_openCloseAnimationCTS?.Cancel();
				_openCloseAnimationCTS = new CancellationTokenSource();
				var token = _openCloseAnimationCTS.Token;

				UniTask.WhenAll(
					PlayPanelCloseAnimation(token),
					_background.FadeAsync(token)
				)
				.ContinueWith(() => _panel.gameObject.SetActive(false))
				.Forget();

				IsOpened = false;
			}
		}

		private UniTask PlayPanelOpenAnimation(CancellationToken token)
		{
			return _panel.DOMoveX(0f, _duration)
				.SetEase(_easeOpen)
				.ToUniTask(token);
		}

		private UniTask PlayPanelCloseAnimation(CancellationToken token)
		{
			return _panel.DOMoveX(-_panel.sizeDelta.x, _duration)
				.SetEase(_easeClose)
				.ToUniTask(token);
		}
	}
}
