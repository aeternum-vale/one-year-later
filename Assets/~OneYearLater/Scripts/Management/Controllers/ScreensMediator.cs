using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class ScreensMediator : IScreensMediator
	{
		[Inject] private FeedScreenController _feedScreenController;
		[Inject] private ExternalStoragesScreenController _externalStoragesScreenController;
		[Inject] private RecordEditorScreenController _recordEditorScreenController;
		[Inject] private ImportScreenController _importScreenController;


		private IScreensMenuView _screensMenu;
		[Inject] private IViewManager _viewManager;

		public ScreensMediator(IScreensMenuView screensMenu)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> ctor");
			_screensMenu = screensMenu;
			_screensMenu.ScreenChangeIntent += OnScreenChangeIntent;
		}

		private async void OnScreenChangeIntent(object sender, EScreenViewKey screenKey)
		{
			_screensMenu.Close();
			_viewManager.SetScreenView(screenKey);
			
			switch (screenKey)
			{
				case EScreenViewKey.Feed:
					await _feedScreenController.DisplayFeedFor(_feedScreenController.CurrentDate);
					break;
			}

		}

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

		public async UniTask ActivateFeedScreen()
		{
			await _feedScreenController.DisplayFeedFor(_feedScreenController.CurrentDate);
			_viewManager.SetScreenView(EScreenViewKey.Feed);
		}

		public UniTask ActivateFeedScreenForToday()
		{
			return ActivateFeedScreenFor(DateTime.Now);
		}

		public async UniTask ActivateFeedScreenFor(DateTime date)
		{
			await _feedScreenController.DisplayFeedFor(date);
			_viewManager.SetScreenView(EScreenViewKey.Feed);
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

	}
}