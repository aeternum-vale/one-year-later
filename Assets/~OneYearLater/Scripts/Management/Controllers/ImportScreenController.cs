using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.Interfaces.Importers;
using UniRx;
using UnityEngine;
using Zenject;

using static Utilities.Utils;
namespace OneYearLater.Management.Controllers
{
	public class ImportScreenController
	{
		[Inject] IPopupManager _popupManager;

		private IImportScreenView _view;
		private IDiaryImporter _diaryImporter;
		private IConversationImporter _conversationImporter;

		public bool IsImportingAllowed
		{
			get => _view.IsImportingAllowed;
			set => _view.IsImportingAllowed = value;
		}

		public ImportScreenController(
			IImportScreenView view,
			IDiaryImporter diaryImporter,
			IConversationImporter conversationImporter
			)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> ctor");
			_view = view;
			_diaryImporter = diaryImporter;
			_conversationImporter = conversationImporter;

			_view.ImportIntent += OnImportIntent;
			_diaryImporter.ImportFromTextFileProgress.Subscribe(p => _view.SetImportFileProgress(EImportType.DiaryFromTxt,p));
			_conversationImporter.ImportFromTextFileProgress.Subscribe(p => _view.SetImportFileProgress(EImportType.ConversationFromTxt,p));
		}

		private async void OnImportIntent(object sender, EImportType type)
		{
			_view.SetIsImportInProgress(type, true);
			ImportResult result;

			switch (type)
			{
				case EImportType.DiaryFromTxt:
					result = await _diaryImporter.ImportFromTextFile();

					if (!result.IsCanceled)
					{
						_popupManager.RunMessagePopupAsync(
							CreateMultiline(
								"Importint diary from text file results",
								$"Imported records count: {result.ImportedRecordsCount}",
								$"Aborted duplicates count: {result.AbortedDuplicatesCount}"),
							"Great!").Forget();
					}
					break;
				case EImportType.ConversationFromTxt:
					result = await _conversationImporter.ImportFromTextFile();

					if (!result.IsCanceled)
					{
						_popupManager.RunMessagePopupAsync(
							CreateMultiline(
								"Importint conversation from text file results",
								$"Imported records count: {result.ImportedRecordsCount}",
								$"Aborted duplicates count: {result.AbortedDuplicatesCount}"),
							"Great!").Forget();
					}

					break;
			}



			_view.SetIsImportInProgress(type, false);
		}
	}
}