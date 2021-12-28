using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Keiwando.NFSO;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.LocalStorage;
using OneYearLater.Management.ViewModels;
using UnityEngine;
using Zenject;

namespace OneYearLater.Import
{
	public class Importer : IImporter
	{
		[Inject] private IPopupManager _popupManager;
		[Inject] private HandledLocalStorage _localRecordStorage;

		private bool _isImportingInProcess;

		private UniTask WaitUntilImportingIsNotInProcess()
		{
			if (_isImportingInProcess)
				return UniTask.WaitUntil(() => !_isImportingInProcess);

			return UniTask.CompletedTask;
		}

		public async UniTask ImportFromTextFile()
		{
			await WaitUntilImportingIsNotInProcess();
			_isImportingInProcess = true;

			bool isCompleted = false;

			(bool isOpened, OpenedFile file) = await TryOpenFile();
			isCompleted = !isOpened;

			if (!isCompleted)
			{
				//string pattern = await _popupManager.RunPromptPopupAsync("Enter records pattern", "e.g. {DAY}-{MONTH}-{YEAR}{NEWLINE}{TEXT}");
				await Parse(file.Data);
			}

			_isImportingInProcess = false;
		}

		private UniTask<(bool, OpenedFile)> TryOpenFile()
		{
			var utcs = new UniTaskCompletionSource<(bool, OpenedFile)>();

			NativeFileSO.shared.OpenFile(
				new SupportedFileType[] { SupportedFileType.PlainText },
				(isOpened, file) =>
				{
					if (isOpened)
					{
						Debug.Log($"<color=lightblue>{GetType().Name}:</color> file.Name={file.Name}");
						utcs.TrySetResult((true, file));
					}
					else
						utcs.TrySetResult((false, null));
				});

			// when complete, call utcs.TrySetResult();
			// when failed, call utcs.TrySetException();
			// when cancel, call utcs.TrySetCanceled();

			return utcs.Task;
		}

		private async UniTask Parse(byte[] bytes)
		{
			using var stream = new MemoryStream(bytes);
			using var streamReader = new StreamReader(stream);
			string line;

			string recordText = string.Empty;
			DateTime? currentDateTime = null;

			while ((line = streamReader.ReadLine()) != null)
			{
				Debug.Log($"<color=lightblue>{GetType().Name}:</color> line={line}");
				if (DateTime.TryParse(line, out DateTime date))
				{
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{line}' is a date!");
					if (currentDateTime.HasValue)
						await InsertNewRecordToDb(currentDateTime.Value, recordText);

					currentDateTime = date;
					recordText = string.Empty;
				}
				else
				{
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{line}' is not a date");

					if (currentDateTime.HasValue)
						recordText += line;
				}
			}

			await InsertNewRecordToDb(currentDateTime.Value, recordText);
		}

		private async UniTask InsertNewRecordToDb(DateTime dateTime, string text)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> creating new record with date={dateTime} and text='{text}'");
			text = text.Trim();
			try
			{
				await _localRecordStorage.InsertRecordAsync(new DiaryRecordViewModel(dateTime, text));
			}
			catch (RecordDuplicateException)
			{
				Debug.Log($"<color=lightblue>{GetType().Name}:</color> the record is already exist");
			}
		}
	}

}