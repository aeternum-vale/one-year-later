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
		private LocalStorageWithExceptionHandling _localRecordStorage;

		[Inject]
		private IScreensMediator _screensMediator;

		public DateTime CurrentDate { get; private set; }

		public FeedScreenController(IFeedScreenView feedScreenView)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> ctor");
			_view = feedScreenView;

			_view.DayChanged += OnDayChanged;
			_view.AddRecordIntent += OnAddRecordIntent;
			_view.EditRecordIntent += OnEditRecordIntent;
		}

		private void OnDayChanged(object sender, DateTime date)
		{
			DisplayFeedFor(date).Forget();
		}

		public async UniTask DisplayFeedFor(DateTime date)
		{
			CurrentDate = date;

			_view.SetIsDatePickingBlocked(true);
			_view.DisplayThatFeedIsLoading();
			var records = await _localRecordStorage.GetAllDayRecordsAsync(date);
			await _view.DisplayDayFeedAsync(date, records);
			_view.SetIsDatePickingBlocked(false);
		}

		private void OnAddRecordIntent(object sender, EventArgs args)
		{
			_screensMediator.ActivateRecordEditorScreenInBlankMode();
		}

		private void OnEditRecordIntent(object sender, int recordId)
		{
			_screensMediator.ActivateRecordEditorScreen(recordId);
		}
	}
}