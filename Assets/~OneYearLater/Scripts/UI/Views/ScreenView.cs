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

		public UniTask Fade() =>
			_canvasGroup.DOFade(0f, ScreenViewFadeDuration).ToUniTask();

		public UniTask Unfade() =>
			_canvasGroup.DOFade(1f, ScreenViewFadeDuration).ToUniTask();

		public UniTask Fade(CancellationToken token) =>
			_canvasGroup.DOFade(0f, ScreenViewFadeDuration).ToUniTask(token);

		public UniTask Unfade(CancellationToken token) =>
			_canvasGroup.DOFade(1f, ScreenViewFadeDuration).ToUniTask(token);


#if UNITY_EDITOR
		private void OnValidate()
		{
			PopulateFields();
		}
#endif
	}
}
