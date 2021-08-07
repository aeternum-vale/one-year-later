using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using OneYearLater.UI.Interfaces;
using UnityEngine;
using static OneYearLater.UI.Constants;
using static Utilities.Extensions;

namespace OneYearLater.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupFader : MonoBehaviour, IAsyncFadable
	{
		public float FadeDuration { get => _fadeDuration; set => _fadeDuration = value; }

		[SerializeField] private float _fadeDuration = 1f;
		[SerializeField] [ReadOnly] private CanvasGroup _canvasGroup;



		#region Unity Callbacks
		private void Start()
		{
			PopulateFields();
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
			if (_canvasGroup == null)
				_canvasGroup = GetComponent<CanvasGroup>();
		}

		public void SetAlpha(float alpha)
		{
			_canvasGroup.alpha = alpha;
		}

		public async UniTask FadeAsync()
		{
			await _canvasGroup.DOFade(0f, _fadeDuration).ToUniTask();
			gameObject.SetActive(false);
		}

		public UniTask UnfadeAsync()
		{
			gameObject.SetActive(true);
			return _canvasGroup.DOFade(1f, _fadeDuration).ToUniTask();
		}

		public async UniTask FadeAsync(CancellationToken token)
		{
			await _canvasGroup.DOFade(0f, _fadeDuration).ToUniTask(token);
			gameObject.SetActive(false);
		}

		public UniTask UnfadeAsync(CancellationToken token)
		{
			gameObject.SetActive(true);
			return _canvasGroup.DOFade(1f, _fadeDuration).ToUniTask(token);
		}
	}
}