using OneYearLater.Management.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management
{
	public class App : MonoBehaviour
	{
		[Inject] private IScreensMediator _screensMediator;
		[Inject] private IRecordStorageConnector _recordStorageConnector;
		[Inject] private IPopupManager _popupManager;


		private async void Start()
		{
			Screen.fullScreen = false;

			var result = await _recordStorageConnector.InitDatabase();
			await _screensMediator.InitializeScreens();

			switch (result)
			{
				case EInitResult.NoDatabase:
					await _popupManager.RunMessagePopupAsync("Hello and welcome!", "Hi!");
					break;
				case EInitResult.InvalidDatabase:
					await _popupManager.RunMessagePopupAsync("Database has been restored after corruption and now empty. Your data may be lost, try to synchronize with some of the external storages to restore your data.");
					break;
			}
		}

	}
}
