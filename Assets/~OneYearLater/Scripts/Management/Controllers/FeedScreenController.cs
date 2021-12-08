using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class FeedScreenController
	{
		private IFeedScreenView _view;
		[Inject] private ILocalRecordStorage _localRecordStorage;
		[Inject] private IViewManager _viewManager;

		public FeedScreenController(IFeedScreenView feedScreenView)
		{
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
			_view.SetIsDatePickingBlocked(true);
			_view.DisplayThatFeedIsLoading();
			await _view.DisplayDayFeedAsync(date, await _localRecordStorage.GetAllDayRecordsAsync(date));
			_view.SetIsDatePickingBlocked(false);
		}

		private void OnAddRecordButtonClicked(object sender, EventArgs args)
		{
			_viewManager.SetScreenView(EScreenViewKey.RecordEditor);
		}
	}

}