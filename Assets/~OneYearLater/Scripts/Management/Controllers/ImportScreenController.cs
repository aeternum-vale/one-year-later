using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using UniRx;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class ImportScreenController
	{
		[Inject] IPopupManager _popupManager;

		private IImportScreenView _view;
		IImporter _importer;

		public ImportScreenController(IImportScreenView view, IImporter importer)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> ctor");
			_view = view;
			_importer = importer;

			_view.ImportFromTextFileIntent += OnImportFromTextFileIntent;
			_importer.ImportFromTextFileProgress.Subscribe(OnImportFromTextFileProgressChange);
		}

		private void OnImportFromTextFileProgressChange(float progress)
		{
			_view.SetImportFromTextFileProgressValue(progress);
		}

		private async void OnImportFromTextFileIntent(object sender, EventArgs args)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnImportFromTextFileIntent");

			_view.SetIsImportFromTextFileInProgress(true);
			var result = await _importer.ImportFromTextFile();

			if (!result.IsCanceled)
			{
				_popupManager.RunMessagePopupAsync($@"Import results
Imported records count: {result.ImportedRecordsCount}
Aborted duplicates count: {result.AbortedDuplicatesCount}", "Great!").Forget();

			}

			_view.SetIsImportFromTextFileInProgress(false);
		}
	}
}