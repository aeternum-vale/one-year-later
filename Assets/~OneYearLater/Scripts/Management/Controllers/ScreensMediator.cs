using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class ScreensMediator : IScreensMediator
	{
		[Inject] private FeedScreenController _feedScreenController; 
		[Inject] private ExternalStoragesScreenController _externalStoragesScreenController; 
		[Inject] private RecordEditorScreenController _recordEditorScreenController; 
		[Inject] private IViewManager _viewManager;


		public async UniTask InitializeScreens()
		{
			await ActivateFeedScreenForToday();
			await _externalStoragesScreenController.InitEachExternalStorage();

			_viewManager.UnblockScreen();
		}

		public UniTask ActivateExternalStoragesScreens()
		{
			_viewManager.SetScreenView(EScreenViewKey.ExternalStorages);
			return UniTask.CompletedTask;
		}

		public UniTask ActivateFeedScreenForToday()
		{
			return ActivateFeedScreenFor(DateTime.Now);
		}

		public UniTask ActivateFeedScreenFor(DateTime date)
		{
			_viewManager.SetScreenView(EScreenViewKey.Feed);
			return _feedScreenController.DisplayFeedFor(date);
		}

		public async UniTask ActivateRecordEditorScreen(int recordId)
		{
			await _recordEditorScreenController.SetEditRecordMode(recordId);
			_viewManager.SetScreenView(EScreenViewKey.RecordEditor);
		}

		public UniTask ActivateRecordEditorScreenInBlankMode()
		{
			_recordEditorScreenController.SetCreateRecordMode();
			_viewManager.SetScreenView(EScreenViewKey.RecordEditor);
			return UniTask.CompletedTask; 
		}

		public UniTask ActivateFeedScreen()
		{
			_viewManager.SetScreenView(EScreenViewKey.Feed);
			return _feedScreenController.DisplayFeedFor(_feedScreenController.CurrentDate);
		}
	}
}