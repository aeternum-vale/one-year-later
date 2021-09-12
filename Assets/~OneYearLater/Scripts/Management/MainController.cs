using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UnityEngine;
using Zenject;

using static Utilities.Utils;

namespace OneYearLater.Management
{
	public class MainController : MonoBehaviour
	{
		[Inject] private IViewManager _viewManager;
		[Inject] private ILocalStorage _localStorage;
		[Inject] private IExternalStorage[] _externalStorages;
		private Dictionary<EExternalStorageKey, IExternalStorage> _externalStorageDict;

		private void Awake()
		{
			_viewManager.DayChanged += OnViewManagerDayChanged;
			_viewManager.ConnectToExternalStorageButtonClicked += OnConnectToExternalStorageButtonClicked;
			_viewManager.SynchronizeWithExternalStorageButtonClicked += OnSynchronizeWithExternalStorageButtonClicked;

			_externalStorageDict = _externalStorages.ToDictionary((es) => es.Key);
		}

		private void Start()
		{
			ExternalStorageViewModel[] vms =
				_externalStorages
					.ToList()
					.Select(es => new ExternalStorageViewModel() { key = es.Key, name = es.Name })
					.ToArray();
			_viewManager.ProvideExternalStorageViewModels(vms, EExternalStorageViewAppearanceState.NotConnected, "not connected");

			DisplayFeedFor(DateTime.Now);
		}

		private async void OnConnectToExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			_viewManager.ChangeExternalStorageViewAppearance(key, EExternalStorageViewAppearanceState.Connecting, "connecting...");
			IExternalStorage es = _externalStorageDict[key];
			es.RequestAccessCode();
			await Delay(2f);
			string accessCode = await _viewManager.ShowPromptPopupAsync($"Paste access code for {es.Name} here", "Enter", "");
			bool success = await es.Connect(accessCode);
			if (success)
				_viewManager.ChangeExternalStorageViewAppearance(key, EExternalStorageViewAppearanceState.Connected, "connected");
			else
				_viewManager.ChangeExternalStorageViewAppearance(key, EExternalStorageViewAppearanceState.NotConnected, "error while connection");
		}

		private async void OnSynchronizeWithExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			IExternalStorage es = _externalStorageDict[key];
			_viewManager.ChangeExternalStorageViewAppearance(key, EExternalStorageViewAppearanceState.Synchronizing, "syncing...");
			bool success = await _localStorage.SynchronizeLocalAndExternal(es);
			if (success)
				_viewManager.ChangeExternalStorageViewAppearance(key, EExternalStorageViewAppearanceState.Connected, $"last sync: {DateTime.Now:g}");
			else
				_viewManager.ChangeExternalStorageViewAppearance(key, EExternalStorageViewAppearanceState.NotConnected, "error while syncing");
		}

		private void OnViewManagerDayChanged(object sender, DateTime date)
		{
			DisplayFeedFor(date);
		}

		private async void DisplayFeedFor(DateTime date)
		{
			_viewManager.SetIsDatePickingBlocked(true);
			_viewManager.DisplayFeedLoading();
			await _viewManager.DisplayDayFeedAsync(date, await _localStorage.GetAllDayRecordsAsync(date));
			_viewManager.SetIsDatePickingBlocked(false);
		}
	}
}
