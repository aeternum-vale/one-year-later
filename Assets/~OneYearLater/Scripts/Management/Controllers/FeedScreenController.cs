using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.LocalStorage;
using Utilities;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class FeedScreenController
	{
		private IFeedScreenView _view;

		[Inject]
		private HandledLocalStorage _localRecordStorage;

		[Inject]
		private IScreensMediator _screensMediator;

		public DateTime CurrentDate { get; private set; }

		public FeedScreenController(IFeedScreenView view)
		{
			_view = view;

			_view.DayChangeIntent += OnDayChanged;
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
		public UniTask DisplayFeedForCurrentDate() =>
			DisplayFeedFor(CurrentDate);


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