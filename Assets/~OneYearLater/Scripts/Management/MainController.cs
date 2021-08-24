using System;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management
{
	public class MainController : MonoBehaviour
	{

		[Inject] private IViewManager _viewManager;
		[Inject] private ILocalStorage _storage;
		[Inject] private IExternalStorage[] _externalStorages;

		private void Awake()
		{
			_viewManager.DayChanged += OnViewManagerDayChanged;
		}

		private void Start() {
			DisplayFeedFor(DateTime.Now);
		}

		private void OnViewManagerDayChanged(object sender, DateTime date)
		{
			DisplayFeedFor(date);
		}

		private async void DisplayFeedFor(DateTime date)
		{
			_viewManager.SetIsDatePickingBlocked(true);
			_viewManager.DisplayFeedLoading();
			await _viewManager.DisplayDayFeedAsync(date, await _storage.GetAllDayRecordsAsync(date));
			_viewManager.SetIsDatePickingBlocked(false);
		}
	}
}
