using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public enum EEditorMode { None = 0, Create = 1, Edit = 2 }
	public class RecordEditorScreenController
	{
		public EEditorMode Mode { get; private set; } = EEditorMode.None;

		[Inject] private IScreensMediator _screensMediator;
		
		[Inject(Id = Constants.HandledRecordStorageId)] 
		private ILocalRecordStorage _localRecordStorage;
		[Inject] private IPopupManager _popupManager;

		private IRecordEditorScreenView _view;
		private int _editingRecordId;

		public RecordEditorScreenController(IRecordEditorScreenView view)
		{
			_view = view;
			_view.ApplyIntent += OnApplyIntent;
			_view.CancelIntent += OnCancelIntent;
			_view.DeleteIntent += OnDeleteIntent;

			_view.DateTime = DateTime.Now;
		}

		private void OnApplyIntent(object sender, EventArgs args)
		{
			switch (Mode)
			{
				case EEditorMode.Create:
					CreateRecord();
					break;
				case EEditorMode.Edit:
					EditRecord();
					break;
				default:
					throw new Exception("invalid editor mode");
			}
		}

		private void OnCancelIntent(object sender, EventArgs args)
		{
			_screensMediator.ActivateFeedScreen().Forget();
		}

		private async void OnDeleteIntent(object sender, EventArgs args)
		{
			if (await _popupManager.RunConfirmPopupAsync("Are you sure you want to delete this record?"))
			{
				await _localRecordStorage.DeleteRecordAsync(_editingRecordId);
				_screensMediator.ActivateFeedScreen().Forget();
			}
		}

		public async UniTask SetEditRecordMode(int recordId)
		{
			Mode = EEditorMode.Edit;
			_editingRecordId = recordId;

			var record = await _localRecordStorage.GetRecordAsync(recordId);

			_view.DateTime = record.DateTime;

			switch (record.Type)
			{
				case ERecordType.Diary:
					var diaryRecord = (DiaryRecordViewModel)record;
					_view.Text = diaryRecord.Text;
					break;
			}
		}

		public void SetCreateRecordMode()
		{
			Mode = EEditorMode.Create;
			_view.DateTime = DateTime.Now;
			_view.Text = string.Empty;
		}

		private async void CreateRecord()
		{
			if (string.IsNullOrWhiteSpace(_view.Text)) return;

			await _localRecordStorage.InsertRecordAsync(
				new DiaryRecordViewModel(_view.DateTime, _view.Text));
			_screensMediator.ActivateFeedScreenForToday().Forget();
		}

		private async void EditRecord()
		{
			await _localRecordStorage.UpdateRecordAsync(
				new DiaryRecordViewModel(_editingRecordId, _view.DateTime, _view.Text));
			_screensMediator.ActivateFeedScreenFor(_view.DateTime).Forget();
		}
	}
}