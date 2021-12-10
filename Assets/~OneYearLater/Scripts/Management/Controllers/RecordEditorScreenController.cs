
using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class RecordEditorScreenController
	{
		private IRecordEditorScreenView _view;

		[Inject] 
		private IScreensMediator _screensMediator;
		
		[Inject] 
		private ILocalRecordStorage _localRecordStorage;


		public RecordEditorScreenController(IRecordEditorScreenView view)
		{
			_view = view;
			_view.ApplyButtonClicked += OnApplyButtonClicked;
			_view.CancelButtonClicked += OnCancelButtonClicked;

			_view.DateTime = DateTime.Now;
		}

		private async void OnApplyButtonClicked(object sender, EventArgs args)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnApplyButtonClicked");
			await _localRecordStorage.SaveRecordsAsync(
				new[] { new DiaryRecordViewModel(_view.DateTime, _view.Text) }
			);
			_screensMediator.ActivateFeedScreenForToday().Forget();
		}

		private void OnCancelButtonClicked(object sender, EventArgs args)
		{
			_screensMediator.ActivateFeedScreenForToday().Forget();
		}

		public async UniTask GoToExistingRecordEditingMode(int recordId)
		{
			var now = DateTime.Now;
			_view.DateTime = now;
			var record = await _localRecordStorage.GetRecordAsync(recordId);

			_view.DateTime = record.DateTime;

			switch (record.Type)
			{
				case ERecordKey.Diary:
					var diaryRecord = (DiaryRecordViewModel)record;
					_view.Text = diaryRecord.Text;
					break;
			}
		}

		public void GoToBlankRecordMode()
		{
			_view.DateTime = DateTime.Now;
			_view.Text = string.Empty;
		}

	}
}