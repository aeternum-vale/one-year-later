using System;
using OneYearLater.Management.Controllers;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management
{
	public class App : MonoBehaviour
	{
		[Inject] private IViewManager _viewManager;
		[Inject] private ILocalRecordStorage _localRecordStorage;
		[Inject] private IAppLocalStorage _appLocalStorage;

		[Inject] private IPopupManager _popupManager;
		[Inject] private Importer _importer;
		[Inject] private FeedScreenController _feedScreenController;
		[Inject] private ExternalStoragesScreenController _externalStoragesScreenController;

		
		[Inject] private IImportScreenView _importScreen;


		private void Awake()
		{
			AddListeners();
		}

		private void AddListeners()
		{
			_importScreen.ImportFromTextFileButtonClick += OnImportFromTextFileButtonClick;
		}

		private async void Start()
		{
			await _externalStoragesScreenController.InitEachExternalStorage();
			await _feedScreenController.DisplayFeedFor(DateTime.Now);

			Debug.Log($"<color=lightblue>{GetType().Name}:</color> Initiated");

			_viewManager.UnblockScreen();
		}


		private void OnImportFromTextFileButtonClick(object sender, EventArgs args)
		{
			_importer.ImportFromTextFile();
		}
	}
}
