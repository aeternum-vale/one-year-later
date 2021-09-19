using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UniRx;
using UnityEngine;
using Zenject;

using static Utilities.Utils;

namespace OneYearLater.Management
{
	public class MainController : MonoBehaviour
	{
		[Inject] private IViewManager _viewManager;
		[Inject] private ILocalRecordStorage _localRecordStorage;
		[Inject] private IAppLocalStorage _appLocalStorage;
		[Inject] private IExternalRecordStorage[] _externalStorages;
		private Dictionary<EExternalStorageKey, IExternalRecordStorage> _externalStorageDict;
		private UniTask _externalStoragePersistentStateSavingTask = UniTask.CompletedTask;

		private void Awake()
		{
			_viewManager.DayChanged += OnViewManagerDayChanged;
			_viewManager.ConnectToExternalStorageButtonClicked += OnConnectToExternalStorageButtonClicked;
			_viewManager.SyncWithExternalStorageButtonClicked += OnSyncWithExternalStorageButtonClicked;

			_externalStorageDict = _externalStorages.ToDictionary((es) => es.Key);
		}

		private async void Start()
		{
			ExternalStorageViewModel[] vms =
				_externalStorages
					.ToList()
					.Select(es => new ExternalStorageViewModel() { key = es.Key, name = es.Name })
					.ToArray();
			_viewManager.ProvideExternalStorageViewModels(vms, EExternalStorageAppearance.NotConnected, "not connected");

			foreach (var es in _externalStorages)
			{
				string stateJson = await _appLocalStorage.GetExternalStorageStateAsync(es.Key);
				es.Init(stateJson);
				es.PersistentState.Subscribe(ps => OnExternalStoragePersistentStateChanged(es.Key, ps));

				if (await es.IsConnected())
					_viewManager.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.Connected, "connected");
			}

			DisplayFeedFor(DateTime.Now);
		}

		private void OnExternalStoragePersistentStateChanged(EExternalStorageKey key, string ps)
		{
			_externalStoragePersistentStateSavingTask =
				_externalStoragePersistentStateSavingTask.ContinueWith(
					() => _appLocalStorage.SaveExternalStorageStateAsync(
						new ExternalStorageModel() { key = key, serializedData = ps })
				);
		}

		private async void OnConnectToExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.Connecting, "connecting...");
			IExternalRecordStorage es = _externalStorageDict[key];
			es.RequestAccessCode();
			await Delay(2f);
			string accessCode = await _viewManager.ShowPromptPopupAsync($"Paste access code for {es.Name} here", "Enter", "");
			bool success = await es.Connect(accessCode);
			if (success)
				_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.Connected, "connected");
			else
				_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.NotConnected, "error while connection");
		}

		private async void OnSyncWithExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			IExternalRecordStorage es = _externalStorageDict[key];
			_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.Synchronizing, "syncing...");
			bool success = await _localRecordStorage.SyncLocalAndExternalRecordStorages(es);
			if (success)
				_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.Connected, $"last sync: {DateTime.Now:g}");
			else
				_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.NotConnected, "error while syncing");
		}

		private void OnViewManagerDayChanged(object sender, DateTime date)
		{
			DisplayFeedFor(date);
		}

		private async void DisplayFeedFor(DateTime date)
		{
			_viewManager.SetIsDatePickingBlocked(true);
			_viewManager.DisplayFeedLoading();
			await _viewManager.DisplayDayFeedAsync(date, await _localRecordStorage.GetAllDayRecordsAsync(date));
			_viewManager.SetIsDatePickingBlocked(false);
		}
	}
}
