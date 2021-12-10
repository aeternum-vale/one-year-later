using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class FeedScreenController
	{
		private IFeedScreenView _view;

		[Inject] 
		private ILocalRecordStorage _localRecordStorage;
		
		[Inject] 
		private IScreensMediator _screensMediator;

		public FeedScreenController(IFeedScreenView feedScreenView)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> ctor");
			_view = feedScreenView;

			_view.DayChanged += OnDayChanged;
			_view.AddRecordButtonClicked += OnAddRecordButtonClicked;
		}

		private void OnDayChanged(object sender, DateTime date)
		{
			DisplayFeedFor(date).Forget();
		}

		public async UniTask DisplayFeedFor(DateTime date)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> DisplayFeedFor {date} _view={_view}");

			_view.SetIsDatePickingBlocked(true);
			_view.DisplayThatFeedIsLoading();
			var records = await _localRecordStorage.GetAllDayRecordsAsync(date);
			await _view.DisplayDayFeedAsync(date, records);
			_view.SetIsDatePickingBlocked(false);
		}

		private void OnAddRecordButtonClicked(object sender, EventArgs args)
		{
			_screensMediator.ActivateRecordEditorScreenInBlankMode();
		}
	}

}