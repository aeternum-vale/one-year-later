using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using OneYearLater.UI.Interfaces;

namespace OneYearLater.UI
{
	[RequireComponent(typeof(CanvasGroupFader))]
	public class ScreenView : MonoBehaviour, IAsyncFadable
	{
		[SerializeField] [ReadOnly] private CanvasGroupFader _canvasGroupFader;

		#region Unity Callbacks
		private void Awake()
		{
			PopulateFields();

			_canvasGroupFader.FadeDuration = Constants.ScreenViewFadeDuration;
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
		}

		public UniTask FadeAsync() => _canvasGroupFader.FadeAsync();
		public UniTask UnfadeAsync() => _canvasGroupFader.UnfadeAsync();
		public UniTask FadeAsync(CancellationToken token) => _canvasGroupFader.FadeAsync(token);
		public UniTask UnfadeAsync(CancellationToken token) => _canvasGroupFader.UnfadeAsync(token);
	}
}
