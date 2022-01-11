using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Keiwando.NFSO;
using OneYearLater.Management;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UniRx;
using UnityEngine;
using Zenject;

namespace OneYearLater.Import
{
	public abstract class TextFileImporter 
	{
		[Inject(Id = Constants.HandledRecordStorageId)]
		private ILocalRecordStorage _localRecordStorage;

		private int _importedRecordsCount = 0;
		private int _abortedDuplicatesCount = 0;

		private ReactiveProperty<float> _importFromTextFileProgress = new ReactiveProperty<float>(0);
		public ReactiveProperty<float> ImportFromTextFileProgress => _importFromTextFileProgress;

		private ReactiveProperty<bool> _isImportingInProcess = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsImportingInProcess => _isImportingInProcess;


		public async UniTask<ImportResult> ImportFromTextFile()
		{
			await WaitUntilImportingIsNotInProcess();
			_isImportingInProcess.Value = true;
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

			_isImportingInProcess.Value = false;
			return result;
		}

		private UniTask WaitUntilImportingIsNotInProcess()
		{
			if (_isImportingInProcess.Value)
				return UniTask.WaitUntil(() => !_isImportingInProcess.Value);

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

		protected IEnumerable<string> GetLines(byte[] bytes)
		{
			using var stream = new MemoryStream(bytes);
			using var streamReader = new StreamReader(stream);
			string line;

			while ((line = streamReader.ReadLine()) != null)
			{
				yield return line;
				int lineBytesCount = Encoding.UTF8.GetBytes(line).Length;
				_importFromTextFileProgress.Value += (float)lineBytesCount / (float)bytes.Length;
			}
		}

		protected abstract UniTask Parse(byte[] bytes);

		protected async UniTask InsertNewRecordToDb(BaseRecordViewModel recordVM)
		{
			recordVM.IsImported = true;

			try
			{
				await _localRecordStorage.InsertRecordAsync(recordVM);
				_importedRecordsCount++;
			}
			catch (RecordDuplicateException)
			{
				_abortedDuplicatesCount++;
			}
		}

	}

}