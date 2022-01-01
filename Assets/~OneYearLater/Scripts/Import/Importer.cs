using System.Text;
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
using System.Globalization;
using UniRx;

namespace OneYearLater.Import
{
	public class Importer : IImporter
	{
		[Inject] private HandledLocalStorage _localRecordStorage;

		private bool _isImportingInProcess;

		private int _importedRecordsCount = 0;
		private int _abortedDuplicatesCount = 0;

		private ReactiveProperty<float> _importFromTextFileProgress = new ReactiveProperty<float>(0);
		public ReactiveProperty<float> ImportFromTextFileProgress => _importFromTextFileProgress;

		public async UniTask<ImportResult> ImportFromTextFile()
		{
			await WaitUntilImportingIsNotInProcess();
			_isImportingInProcess = true;
			_importedRecordsCount = 0;
			_abortedDuplicatesCount = 0;
			bool isCanceled;

			(bool isOpened, OpenedFile file) = await TryOpenFile();
			isCanceled = !isOpened;

			if (!isCanceled)
			{
				_importFromTextFileProgress.Value = 0f;
				//string pattern = await _popupManager.RunPromptPopupAsync("Enter records pattern", "e.g. {DAY}-{MONTH}-{YEAR}{NEWLINE}{TEXT}");
				await Parse(file.Data);
				_importFromTextFileProgress.Value = 1f;
			}

			var result = new ImportResult()
			{
				IsCanceled = isCanceled,
				ImportedRecordsCount = _importedRecordsCount,
				AbortedDuplicatesCount = _abortedDuplicatesCount,
			};

			_isImportingInProcess = false;
			return result;
		}

		private UniTask WaitUntilImportingIsNotInProcess()
		{
			if (_isImportingInProcess)
				return UniTask.WaitUntil(() => !_isImportingInProcess);

			return UniTask.CompletedTask;
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
			return utcs.Task;
		}

		private async UniTask Parse(byte[] bytes)
		{
			using var stream = new MemoryStream(bytes);
			using var streamReader = new StreamReader(stream);
			string line;

			DateTime currentDateTime = DateTime.MinValue;

			var recordTextSB = new StringBuilder();

			while ((line = streamReader.ReadLine()) != null)
			{

				if (IsLineADate(line, out DateTime parsedDate))
				{
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{line}' is a date!");

					if (recordTextSB.Length > 0)
						await InsertNewRecordToDb(currentDateTime, recordTextSB.ToString());

					if (IsLineATime(line, out DateTime parsedOnlyTime))
					{
						Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{line}' is a time!");

						if (currentDateTime == DateTime.MinValue)
							throw new ImportException("Time line before any date line");

						DateTime c = currentDateTime;
						DateTime t = parsedOnlyTime;

						currentDateTime = new DateTime(c.Year, c.Month, c.Day, t.Hour, t.Minute, t.Second);
					}
					else
						currentDateTime = parsedDate;

					recordTextSB = recordTextSB.Clear();
				}
				else
				{
					string debugLine = (line.Length < 50) ? line : (line.Substring(0, 50) + "...");
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{debugLine}' is not a date");
					recordTextSB.AppendLine(line);
				}


				int lineBytesCount = Encoding.UTF8.GetBytes(line).Length;
				_importFromTextFileProgress.Value += (float)lineBytesCount / (float)bytes.Length;
			}

			await InsertNewRecordToDb(currentDateTime, recordTextSB.ToString());
		}

		private async UniTask InsertNewRecordToDb(DateTime dateTime, string text)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> creating new record with date={dateTime} and text='{text}'");

			text = text.Trim();

			try
			{
				await _localRecordStorage.InsertRecordAsync(new DiaryRecordViewModel(dateTime, text));
				_importedRecordsCount++;
			}
			catch (RecordDuplicateException)
			{
				_abortedDuplicatesCount++;
				Debug.Log($"<color=lightblue>{GetType().Name}:</color> the record is already exist");
			}
		}

		private bool IsLineADate(string line, out DateTime date)
		{
			if (DateTime.TryParse(line, out date))
				return true;

			line = line.Replace("года", "");

			return DateTime.TryParse(line, out date);
		}

		private bool IsLineATime(string line, out DateTime date)
		{
			var trimmedLine = line.Trim();

			return DateTime.TryParseExact(trimmedLine, new[] { "HH:mm", "HH:mm:ss", "H:mm", "H:mm:ss" },
				CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
		}
	}

}