using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using static Utilities.Extensions;
using static OneYearLater.UI.Constants;


namespace OneYearLater.UI
{
	public class ScreenView : MonoBehaviour
	{
		[SerializeField] [ReadOnly] private CanvasGroup _canvasGroup;

		private void Start()
		{
			PopulateFields();
		}

		private void PopulateFields()
		{
			if (_canvasGroup == null)
				_canvasGroup = GetComponent<CanvasGroup>();
		}

		public async UniTask FadeAsync()
		{
			await _canvasGroup.DOFade(0f, ScreenViewFadeDuration).ToUniTask();
			gameObject.SetActive(false);
		}

		public UniTask UnfadeAsync()
		{
			gameObject.SetActive(true);
			return _canvasGroup.DOFade(1f, ScreenViewFadeDuration).ToUniTask();
		}

		public async UniTask FadeAsync(CancellationToken token)
		{
			await _canvasGroup.DOFade(0f, ScreenViewFadeDuration).ToUniTask(token);
			gameObject.SetActive(false);
		}

		public  UniTask UnfadeAsync(CancellationToken token)
		{
			gameObject.SetActive(true);
			return _canvasGroup.DOFade(1f, ScreenViewFadeDuration).ToUniTask(token);
		}


#if UNITY_EDITOR
		private void OnValidate()
		{
			PopulateFields();
		}
#endif
	}
}
