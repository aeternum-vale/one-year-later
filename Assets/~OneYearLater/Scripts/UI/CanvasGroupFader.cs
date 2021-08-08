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

		[SerializeField] private float _ownFadeDuration = 1f;
		[SerializeField] [ReadOnly] private CanvasGroup _canvasGroup;


		public void SetAlpha(float alpha)
		{
			_canvasGroup.alpha = alpha;
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

		public  UniTask FadeAsync() => FadeAsync(_ownFadeDuration);
		public UniTask UnfadeAsync() => UnfadeAsync(_ownFadeDuration);
		public UniTask FadeAsync(CancellationToken token) => FadeAsync(_ownFadeDuration, token);
		public UniTask UnfadeAsync(CancellationToken token) => UnfadeAsync(_ownFadeDuration, token);
	}
}