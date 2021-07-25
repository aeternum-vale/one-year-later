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

		private void Awake()
		{
			AddListeners();
		}

		private void Start()
		{
			DisplayFeedFor(DateTime.Today);
		}

		private void AddListeners()
		{
			_viewManager.DayChanged += OnViewManagerDayChanged;
		}

		private void RemoveListeners()
		{
			_viewManager.DayChanged -= OnViewManagerDayChanged;
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
