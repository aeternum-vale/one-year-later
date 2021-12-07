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

		public FeedScreenController(IFeedScreenView feedScreenView)
		{
			_view = feedScreenView;
			AddListeners();
		}

		private void AddListeners()
		{
			_view.DayChanged += OnDayChanged;
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
	}

}