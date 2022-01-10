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
			_view.ConnectToExternalStorageIntent += OnConnectToExternalStorageIntent;
			_view.DisconnectFromExternalStorageIntent += OnDisconnectFromExternalStorageIntent;
			_view.SyncWithExternalStorageIntent += OnSyncWithExternalStorageIntent;
		}

		public async UniTask InitEachExternalStorage()
		{
			ExternalStorageViewModel[] vms =
				_externalStorages
					.ToList()
					.Select(es => new ExternalStorageViewModel() { key = es.Key, name = es.Name })
					.ToArray();

			_view.ProvideExternalStorageViewModels(vms);

			foreach (IExternalStorage es in _externalStorages)
			{
				ExternalStorageViewModel? esvm = await _appLocalStorage.GetExternalStorageViewModel(es.Key);

				es.Init(esvm?.state);
				es.PersistentState.Subscribe(s => OnExternalStorageStateChanged(es.Key, s));

				if (es.IsWaitingForAccessCode)
				{
					await _screensMediator.ActivateExternalStoragesScreens();
					await ShowExternalStorageAccessCodePrompt(es);
					continue;
				}

				await DefineAppearance(es, esvm);
			}
		}

		public void SetWaitingStateForAllExternalStorages()
		{
			foreach (IExternalStorage es in _externalStorages)
				_view.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.Waiting);
		}

		public UniTask DefineStateForAllExternalStorages()
		{
			List<UniTask> tasks = new List<UniTask>();
			foreach (IExternalStorage es in _externalStorages)
				tasks.Add(DefineAppearance(es));
			
			return UniTask.WhenAll(tasks);
		}

		private async UniTask DefineAppearance(IExternalStorage es)
		{
			ExternalStorageViewModel? esvm = await _appLocalStorage.GetExternalStorageViewModel(es.Key);
			await DefineAppearance(es, esvm);
		}

		private async UniTask DefineAppearance(IExternalStorage es, ExternalStorageViewModel? esvm)
		{
			_view.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.Waiting);
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
			else
			{
				_view.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.NotConnected);
			}
		}

		private async void OnConnectToExternalStorageIntent(object sender, EExternalStorageKey key)
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

		private async void OnDisconnectFromExternalStorageIntent(object sender, EExternalStorageKey key)
		{
			IExternalStorage es = _externalStorageDict[key];
			if (await _popupManager.RunConfirmPopupAsync($"Are you sure you want to disconnect from {es.Name}?"))
			{
				await es.Disconnect();
				_view.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.NotConnected);
			}
		}

		private async void OnSyncWithExternalStorageIntent(object sender, EExternalStorageKey key)
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
			{
				_view.ChangeExternalStorageAppearance(
					key, EExternalStorageAppearance.Error, "error while syncing");

				await Delay(3f);

				await DefineAppearance(es);
			}
		}

		private void OnExternalStorageStateChanged(EExternalStorageKey key, string state)
		{
			_externalStorageStateSavingTask = _externalStorageStateSavingTask
				.ContinueWith(() => _appLocalStorage.UpdateExternalStorageStateAsync(key, state));
		}

		private string GetLastSyncStatus(DateTime date) => $"last sync: {date:g}";

	}

}