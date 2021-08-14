using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(CanvasGroupFader))]
	public class ScreenView : MonoBehaviour
	{
		[SerializeField] [ReadOnly] private CanvasGroupFader _canvasGroupFader;

		private void Awake()
		{
			_canvasGroupFader = GetComponent<CanvasGroupFader>();

			_canvasGroupFader.OwnFadeDuration = Constants.ScreenViewFadeDuration;
		}

		public UniTask FadeAsync(CancellationToken token) => _canvasGroupFader.FadeAsync(token);
		public UniTask UnfadeAsync(CancellationToken token) => _canvasGroupFader.UnfadeAsync(token);
	}
}
