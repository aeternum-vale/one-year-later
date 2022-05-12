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
		private string _editingRecordHash;

		public RecordEditorScreenController(IRecordEditorScreenView view)
		{
			_view = view;
			_view.ApplyIntent += OnApplyIntent;
			_view.CancelIntent += OnCancelIntent;
			_view.DeleteIntent += OnDeleteIntent;
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
				await _localRecordStorage.DeleteRecordAsync(_editingRecordHash);
				_screensMediator.ActivateFeedScreen().Forget();
			}
		}

		public async UniTask SetEditRecordMode(string recordHash)
		{
			Mode = EEditorMode.Edit;
			_editingRecordHash = recordHash;

			BaseRecordViewModel recordVM = await _localRecordStorage.GetRecordAsync(recordHash);

			_view.EditingRecordViewModel = recordVM;
		}

		public void SetCreateRecordMode()
		{
			Mode = EEditorMode.Create;
			_view.EditingRecordViewModel = new DiaryRecordViewModel(DateTime.Now, string.Empty);
		}

		private async void CreateRecord()
		{
			await _localRecordStorage.InsertRecordAsync(_view.EditingRecordViewModel);
			_screensMediator.ActivateFeedScreenForToday().Forget();
		}

		private async void EditRecord()
		{
			await _localRecordStorage.UpdateRecordAsync(_view.EditingRecordViewModel);

			_screensMediator.ActivateFeedScreenFor(_view.EditingRecordViewModel.RecordDateTime).Forget();
		}
	}
}