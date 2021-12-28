using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class ImportScreenController
	{
		[Inject] IImporter _importer;
		private IImportScreenView _view;

		public ImportScreenController(IImportScreenView view)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> ctor");
			_view = view;

			_view.ImportFromTextFileIntent += OnImportFromTextFileIntent;
		}

		private void OnImportFromTextFileIntent(object sender, EventArgs args)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnImportFromTextFileIntent");
			_importer.ImportFromTextFile().Forget();
		}
	}
}