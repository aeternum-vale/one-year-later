
using System;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management.Controllers
{
	public class RecordEditorScreenController
	{
		[Inject] private IViewManager _viewManager;
		[Inject] private ILocalRecordStorage _localRecordStorage;

		IRecordEditorScreenView _view;

		public RecordEditorScreenController(IRecordEditorScreenView view)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> ctor, view={view}");

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
			_viewManager.SetScreenView(EScreenViewKey.Feed);
		}

		private void OnCancelButtonClicked(object sender, EventArgs args)
		{
			_viewManager.SetScreenView(EScreenViewKey.Feed);
		}
	}
}