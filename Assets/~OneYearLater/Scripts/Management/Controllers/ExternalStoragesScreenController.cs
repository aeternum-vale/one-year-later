using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using Zenject;
using UniRx;

using static Utilities.Utils;

namespace OneYearLater.Management.Controllers
{
	public class ExternalStoragesScreenController
	{
		[Inject] private IPopupManager _popupManager;
		[Inject] private IRecordStorageSynchronizer _synchronizer;
		[Inject] private IAppLocalStorage _appLocalStorage;
		[Inject] private IExternalStorage[] _externalStorages;
		[Inject] private IScreensMediator _screensMediator;


		private IExternalStoragesScreenView _view;
		private Dictionary<EExternalStorageKey, IExternalStorage> _externalStorageDict;
		private UniTask _externalStorageStateSavingTask = UniTask.CompletedTask;


		public async UniTask InitEachExternalStorage()
		{
			ExternalStorageViewModel[] vms =
				_externalStorages
					.ToList()
					.Select(es => new ExternalStorageViewModel() { key = es.Key, name = es.Name })
					.ToArray();

			_view.ProvideExternalStorageViewModels(vms);

			foreach (var es in _externalStorages)
			{
				var esvm = await _appLocalStorage.GetExternalStorageAsync(es.Key);

				es.Init(esvm?.state);
				es.PersistentState.Subscribe(s => OnExternalStorageStateChanged(es.Key, s));

				if (es.IsWaitingForAccessCode)
				{
					await _screensMediator.ActivateExternalStoragesScreens();
					await ShowExternalStorageAccessCodePrompt(es);
					continue;
				}

				if (await es.IsConnected())
				{
					DateTime? lastSync = esvm?.lastSync;

					if (lastSync != null)
						_view.ChangeExternalStorageAppearance(
							es.Key,
							EExternalStorageAppearance.Connected,
							GetLastSyncStatus(lastSync.Value)
						);
					else
						_view.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.Connected);
				}
			}
		}

		public ExternalStoragesScreenController(
			IExternalStoragesScreenView externalStoragesScreenView,
			IExternalStorage[] externalStorages
		)
		{
			_externalStorages = externalStorages;
			_externalStorageDict = externalStorages.ToDictionary(es => es.Key);
			_view = externalStoragesScreenView;

			AddListeners();
		}

		private void AddListeners()
		{
			_view.ConnectToExternalStorageButtonClicked += OnConnectToExternalStorageButtonClicked;
			_view.DisconnectFromExternalStorageButtonClicked += OnDisconnectFromExternalStorageButtonClicked;
			_view.SyncWithExternalStorageButtonClicked += OnSyncWithExternalStorageButtonClicked;
		}


		private async void OnConnectToExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			_view.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.Connecting);
			IExternalStorage es = _externalStorageDict[key];
			es.RequestAccessCode();
			await Delay(2f);
			ShowExternalStorageAccessCodePrompt(es).Forget();
		}

		private async UniTask ShowExternalStorageAccessCodePrompt(IExternalStorage es)
		{
			string accessCode = await _popupManager.RunPromptPopupAsync($"Paste access code for {es.Name} here", "", "Enter");
			bool success = await es.ConnectWithAccessCode(accessCode);
			if (success)
				_view.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.Connected);
			else
				_view.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.NotConnected);
		}

		private async void OnDisconnectFromExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			IExternalStorage es = _externalStorageDict[key];
			if (await _popupManager.RunConfirmPopupAsync($"Are you sure you want to disconnect from {es.Name}?"))
			{
				await es.Disconnect();
				_view.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.NotConnected);
			}
		}

		private async void OnSyncWithExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			IExternalStorage es = _externalStorageDict[key];
			_view.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.Synchronizing);
			bool success = await _synchronizer.TrySyncLocalAndExternalRecordStorages(es);

			if (success)
			{
				DateTime syncDate = DateTime.Now;
				await _appLocalStorage.UpdateExternalStorageSyncDateAsync(key, syncDate);

				_view.ChangeExternalStorageAppearance(
						key, EExternalStorageAppearance.Connected, GetLastSyncStatus(syncDate));
			}
			else
				_view.ChangeExternalStorageAppearance(
					key, EExternalStorageAppearance.NotConnected, "error while syncing");
		}

		private void OnExternalStorageStateChanged(EExternalStorageKey key, string state)
		{
			_externalStorageStateSavingTask = _externalStorageStateSavingTask
				.ContinueWith(() => _appLocalStorage.UpdateExternalStorageStateAsync(key, state));
		}

		private string GetLastSyncStatus(DateTime date) => $"last sync: {date:g}";

	}

}