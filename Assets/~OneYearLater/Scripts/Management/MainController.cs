using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Keiwando.NFSO;
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
		[Inject] private IExternalStorage[] _externalStorages;
		[Inject] private IPopupManager _popupManager;

		private Dictionary<EExternalStorageKey, IExternalStorage> _externalStorageDict;
		private UniTask _externalStorageStateSavingTask = UniTask.CompletedTask;

		private void Awake()
		{
			_externalStorageDict = _externalStorages.ToDictionary(es => es.Key);

			AddListeners();
		}

		private void AddListeners()
		{
			_viewManager.DayChanged += OnViewManagerDayChanged;
			_viewManager.ConnectToExternalStorageButtonClicked += OnConnectToExternalStorageButtonClicked;
			_viewManager.DisconnectFromExternalStorageButtonClicked += OnDisconnectFromExternalStorageButtonClicked;
			_viewManager.SyncWithExternalStorageButtonClicked += OnSyncWithExternalStorageButtonClicked;
			_viewManager.ImportFromTxtButtonClick += OnImportFromTxtButtonClick;
		}

		private async void Start()
		{
			ExternalStorageViewModel[] vms =
				_externalStorages
					.ToList()
					.Select(es => new ExternalStorageViewModel() { key = es.Key, name = es.Name })
					.ToArray();
			_viewManager.ProvideExternalStorageViewModels(vms);

			foreach (var es in _externalStorages)
			{
				var esm = await _appLocalStorage.GetExternalStorageAsync(es.Key);

				es.Init(esm?.state);
				es.PersistentState.Subscribe(s => OnExternalStorageStateChanged(es.Key, s));

				if (es.IsWaitingForAccessCode)
				{
					_viewManager.SetScreenView(EScreenViewKey.ExternalStorages);
					ShowExternalStorageAccessCodePrompt(es);
					continue;
				}

				if (await es.IsConnected())
				{
					DateTime? lastSync = esm?.lastSync;

					if (lastSync != null)
						_viewManager.ChangeExternalStorageAppearance(
							es.Key,
							EExternalStorageAppearance.Connected,
							GetLastSyncStatus(lastSync.Value)
						);
					else
						_viewManager.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.Connected);
				}
			}

			await DisplayFeedFor(DateTime.Now);

			Debug.Log($"<color=lightblue>{GetType().Name}:</color> App Initiated");

			_viewManager.UnblockScreen();
		}

		private void OnExternalStorageStateChanged(EExternalStorageKey key, string state)
		{
			_externalStorageStateSavingTask = _externalStorageStateSavingTask
				.ContinueWith(() => _appLocalStorage.UpdateExternalStorageStateAsync(key, state));
		}

		private async void OnConnectToExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.Connecting);
			IExternalStorage es = _externalStorageDict[key];
			es.RequestAccessCode();
			await Delay(2f);
			ShowExternalStorageAccessCodePrompt(es);
		}

		private async void OnDisconnectFromExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			IExternalStorage es = _externalStorageDict[key];
			if (await _popupManager.RunConfirmPopupAsync($"Are you sure you want to disconnect from {es.Name}?"))
			{
				await es.Disconnect();
				_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.NotConnected);
			}
		}

		private async void ShowExternalStorageAccessCodePrompt(IExternalStorage es)
		{
			string accessCode = await _popupManager.RunPromptPopupAsync($"Paste access code for {es.Name} here", "Enter", "");
			bool success = await es.ConnectWithAccessCode(accessCode);
			if (success)
				_viewManager.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.Connected);
			else
				_viewManager.ChangeExternalStorageAppearance(es.Key, EExternalStorageAppearance.NotConnected);
		}

		private async void OnSyncWithExternalStorageButtonClicked(object sender, EExternalStorageKey key)
		{
			IExternalStorage es = _externalStorageDict[key];
			_viewManager.ChangeExternalStorageAppearance(key, EExternalStorageAppearance.Synchronizing);
			bool success = await _localRecordStorage.SyncLocalAndExternalRecordStoragesAsync(es);

			if (success)
			{
				DateTime syncDate = DateTime.Now;
				await _appLocalStorage.UpdateExternalStorageSyncDateAsync(key, syncDate);

				_viewManager.ChangeExternalStorageAppearance(
						key, EExternalStorageAppearance.Connected, GetLastSyncStatus(syncDate));
			}
			else
				_viewManager.ChangeExternalStorageAppearance(
					key, EExternalStorageAppearance.NotConnected, "error while syncing");
		}

		private void OnViewManagerDayChanged(object sender, DateTime date)
		{
			DisplayFeedFor(date).Forget();
		}

		private async UniTask DisplayFeedFor(DateTime date)
		{
			_viewManager.SetIsDatePickingBlocked(true);
			_viewManager.DisplayFeedLoading();
			await _viewManager.DisplayDayFeedAsync(date, await _localRecordStorage.GetAllDayRecordsAsync(date));
			_viewManager.SetIsDatePickingBlocked(false);
		}

		private string GetLastSyncStatus(DateTime date) => $"last sync: {date:g}";

		private void OnImportFromTxtButtonClick(object sender, EventArgs args)
		{
			NativeFileSO.shared.OpenFile(
				new SupportedFileType[] { SupportedFileType.PlainText },
				(isOpened, file) =>
				{
					if (isOpened)
					{
						Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnImportFromTxtButtonClick file.Name={file.Name}");

						Debug.Log($"<color=lightblue>{GetType().Name}:</color> content={file.ToUTF8String()}");
					}
					else
					{
						Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnImportFromTxtButtonClick file dialog dismissed");
					}
				});
		}
	}
}
