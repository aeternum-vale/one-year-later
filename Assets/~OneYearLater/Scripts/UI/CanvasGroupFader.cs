using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

using static Utilities.Extensions;

namespace OneYearLater.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupFader : MonoBehaviour
	{
		public float OwnFadeDuration { get => _ownFadeDuration; set => _ownFadeDuration = value; }
		private float _ownFadeDuration = 1f;
		private CanvasGroup _canvasGroup;

		private void Awake() {
			_canvasGroup = GetComponent<CanvasGroup>();
		}

		public void SetAlphaImmediately(float alpha)
		{
			_canvasGroup.alpha = alpha;
		}

		public UniTask SetAlphaAsync(float alpha, float duration)
		{
			return _canvasGroup.DOFade(alpha, duration).ToUniTask();
		}

		public UniTask SetAlphaAsync(float alpha, float duration, CancellationToken token)
		{
			return _canvasGroup.DOFade(alpha, duration).ToUniTask(token);
		}

		public UniTask SetAlphaAsync(float alpha, CancellationToken token)
		{
			return _canvasGroup.DOFade(alpha, OwnFadeDuration).ToUniTask(token);
		}

		public UniTask SetAlphaAsync(float alpha)
		{
			return _canvasGroup.DOFade(alpha, OwnFadeDuration).ToUniTask();
		}

		public async UniTask FadeAsync(float duration)
		{
			await _canvasGroup.DOFade(0f, duration).ToUniTask();
			gameObject.SetActive(false);
		}

		public UniTask UnfadeAsync(float duration)
		{
			gameObject.SetActive(true);
			return _canvasGroup.DOFade(1f, duration).ToUniTask();
		}

		public async UniTask FadeAsync(float duration, CancellationToken token)
		{
			await _canvasGroup.DOFade(0f, duration).ToUniTask(token);
			gameObject.SetActive(false);
		}

		public UniTask UnfadeAsync(float duration, CancellationToken token)
		{
			gameObject.SetActive(true);
			return _canvasGroup.DOFade(1f, duration).ToUniTask(token);
		}

		public UniTask FadeAsync() => FadeAsync(_ownFadeDuration);
		public UniTask UnfadeAsync() => UnfadeAsync(_ownFadeDuration);
		public UniTask FadeAsync(CancellationToken token) => FadeAsync(_ownFadeDuration, token);
		public UniTask UnfadeAsync(CancellationToken token) => UnfadeAsync(_ownFadeDuration, token);
	}
}