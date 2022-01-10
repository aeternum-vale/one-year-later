using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.LocalStorage;
using UnityEngine;
using Zenject;
using UniRx;
using System.Collections.Generic;

namespace OneYearLater.Management.Controllers
{
	public class ScreensMediator : IScreensMediator
	{
		[Inject] private ImportScreenController _importScreenController;
		[Inject] private FeedScreenController _feedScreenController;
		[Inject] private ExternalStoragesScreenController _externalStoragesScreenController;
		[Inject] private RecordEditorScreenController _recordEditorScreenController;
		[Inject] private SettingsScreenController _settingsScreenController;

		[Inject] private RecordStorageUsingWatcher _recordStorageUsingWatcher;


		[Inject] private IScreensMenuView _screensMenu;
		[Inject] private IViewManager _viewManager;


		public async UniTask InitializeScreens()
		{
			_screensMenu.ScreenChangeIntent += OnScreenChangeIntent;
			_recordStorageUsingWatcher.StorageUsingStatusChange += OnStorageUsingStatusChange;

			await ActivateFeedScreenForToday();
			await _externalStoragesScreenController.InitEachExternalStorage();

			_viewManager.UnblockScreen();
		}

		private async void OnScreenChangeIntent(object sender, EScreenViewKey screenKey)
		{
			_screensMenu.Close();
			_viewManager.SetScreenView(screenKey);

			switch (screenKey)
			{
				case EScreenViewKey.Feed:
					await _feedScreenController.DisplayFeedForCurrentDate();
					break;
			}
		}

		public UniTask ActivateExternalStoragesScreens()
		{
			_viewManager.SetScreenView(EScreenViewKey.ExternalStorages);
			return UniTask.CompletedTask;
		}

		public async UniTask ActivateFeedScreen()
		{
			_viewManager.SetScreenView(EScreenViewKey.Feed);
			await _feedScreenController.DisplayFeedForCurrentDate();
		}

		public UniTask ActivateFeedScreenForToday()
		{
			return ActivateFeedScreenFor(DateTime.Now);
		}

		public async UniTask ActivateFeedScreenFor(DateTime date)
		{
			_viewManager.SetScreenView(EScreenViewKey.Feed);
			await _feedScreenController.DisplayFeedFor(date);
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

		private void OnStorageUsingStatusChange(object sender, StorageUsingStatusChangeArgs args)
		{
			switch (args.User)
			{
				case EStorageUser.Importer:
					if (args.IsRecordStorageInUse)
						_externalStoragesScreenController.SetWaitingStateForAllExternalStorages();
					else
						_externalStoragesScreenController.DefineStateForAllExternalStorages();

					break;
				case EStorageUser.Synchronizer:
					_importScreenController.IsImportingAllowed = !args.IsRecordStorageInUse;
					break;
			}
		}

	}
}