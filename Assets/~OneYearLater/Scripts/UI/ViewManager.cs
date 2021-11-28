using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using OneYearLater.UI.Interfaces;
using OneYearLater.UI.Popups;
using OneYearLater.UI.Views;
using OneYearLater.UI.Views.ScreenViews;
using UnityEngine;
using Zenject;

using static Utilities.Extensions;

namespace OneYearLater.UI
{

	public class ViewManager : MonoBehaviour, IViewManager
	{
		public event EventHandler<DateTime> DayChanged;
		public event EventHandler<EExternalStorageKey> ConnectToExternalStorageButtonClicked;
		public event EventHandler<EExternalStorageKey> DisconnectFromExternalStorageButtonClicked;
		public event EventHandler<EExternalStorageKey> SyncWithExternalStorageButtonClicked;

		[Inject] private IMobileInputHandler _mobileInputHandler;


		[SerializeField] private ScreenViewSPair[] _screenViewArray;
		[SerializeField] private ExternalStorageView _externalStorageViewPrefab;
		[SerializeField] private DiaryRecordView _diaryRecordViewPrefab;
		[SerializeField] private SideMenu _sideMenu;
		[SerializeField] private CanvasGroupFader _screenBlocker;
		[SerializeField] private PopupManager _popupManager;


		private Dictionary<EScreenViewKey, ScreenView> _screenViewDictionary;
		private ExternalStorageViewDataDict _externalStoragesViewData
			= new ExternalStorageViewDataDict();
		private FeedScreenView _feedView;
		private ExternalStoragesScreenView _externalStoragesScreenView;
		private EScreenViewKey _currentScreenViewKey = EScreenViewKey.None;
		private CancellationTokenSource _screenViewChangeCTS;


		#region Unity Callbacks
		private void Awake()
		{
			_screenViewArray.ToDictionary(out _screenViewDictionary);

			_feedView = _screenViewDictionary[EScreenViewKey.Feed].GetComponent<FeedScreenView>();
			_externalStoragesScreenView = _screenViewDictionary[EScreenViewKey.ExternalStorages]
				.GetComponent<ExternalStoragesScreenView>();

			_feedView.DayChanged += OnFeedViewDayChanged;

			_mobileInputHandler.SwipeRight += OnSwipeRight;
			_mobileInputHandler.TapOnRightBorder += OnTapOnRightBorder;

			_sideMenu.ScreenViewButtonClick += (s, key) =>
			{
				SetScreenView(key);
				_sideMenu.Close();
			};
		}

		private void Start()
		{
			SetScreenView(EScreenViewKey.Feed);
		}
		#endregion

		public void ProvideExternalStorageViewModels(IEnumerable<ExternalStorageViewModel> viewModels)
		{
			viewModels.ToList().ForEach(vm =>
			{
				ExternalStorageView view = Instantiate(_externalStorageViewPrefab);
				view.MobileInputHandler = _mobileInputHandler;
				view.Text = vm.name;
				_externalStoragesViewData.Add(
					vm.key,
					new ExternalStorageViewData()
					{
						view = view,
						viewModel = vm
					});

				view.ConnectButtonClicked += (s, a) =>
					ConnectToExternalStorageButtonClicked?.Invoke(this, vm.key);

				view.DisconnectButtonClicked += (s, a) =>
					DisconnectFromExternalStorageButtonClicked?.Invoke(this, vm.key);

				view.SyncButtonClicked += (s, a) =>
					SyncWithExternalStorageButtonClicked?.Invoke(this, vm.key);

				view.ChangeAppearance(EExternalStorageAppearance.NotConnected);
			});

			_externalStoragesScreenView.PopulateExternalStoragesList(_externalStoragesViewData);
		}

		public void ChangeExternalStorageAppearance(EExternalStorageKey key, EExternalStorageAppearance appearance, string status = null)
		{
			ExternalStorageView view = _externalStoragesViewData[key].view;
			view.ChangeAppearance(appearance, status);
		}

		private void OnSwipeRight(object sender, bool fromBorder)
		{
			if (fromBorder && !_popupManager.IsAnyPopupActive) _sideMenu.Open();
		}

		private void OnTapOnRightBorder(object sender, EventArgs args)
		{
			if (!_popupManager.IsAnyPopupActive)
				_sideMenu.Close();
		}

		public async UniTask DisplayDayFeedAsync(DateTime date, IEnumerable<BaseRecordViewModel> records)
		{
			_feedView.SetDate(date);

			_feedView.SetIsNoRecordsMessageActive(false);
			_feedView.SetIsLoadingImageActive(false);
			_feedView.ClearRecordsContainer();

			if (records.IsAny())
			{
				_feedView.SetIsLoadingImageActive(true);
				List<GameObject> recordGameObjects = new List<GameObject>();
				foreach (var record in records)
				{
					switch (record.Type)
					{
						case ERecordKey.Diary:

							DiaryRecordView v = Instantiate<DiaryRecordView>(_diaryRecordViewPrefab);
							DiaryRecordViewModel vm = (DiaryRecordViewModel)record;
							v.TimeText = vm.DateTime.ToString("HH:mm");
							v.ContentText = vm.Text;
							recordGameObjects.Add(v.gameObject);
							break;
						default:
							throw new Exception("invalid record type");
					}
				}
				await _feedView.DisplayRecords(recordGameObjects);
				_feedView.SetIsLoadingImageActive(false);
			}
			else
				_feedView.SetIsNoRecordsMessageActive(true);

		}

		public void DisplayFeedLoading()
		{
			_feedView.SetIsNoRecordsMessageActive(false);
			_feedView.ClearRecordsContainer();
			_feedView.SetIsLoadingImageActive(true);
		}

		public void SetIsDatePickingBlocked(bool isBlocked)
		{
			_feedView.SetIsDatePickingBlocked(isBlocked);
		}

		public void SetScreenView(EScreenViewKey screenViewKey)
		{
			_screenViewChangeCTS?.Cancel();
			_screenViewChangeCTS = new CancellationTokenSource();
			var token = _screenViewChangeCTS.Token;

			foreach (var entry in _screenViewDictionary)
				if (entry.Key != screenViewKey && entry.Value.gameObject.activeSelf)
					entry.Value.FadeAsync(token).Forget();

			_screenViewDictionary[screenViewKey].UnfadeAsync(token).Forget();

			_currentScreenViewKey = screenViewKey;
		}

		private void OnFeedViewDayChanged(object sender, DateTime date)
		{
			DayChanged?.Invoke(this, date);
		}


		public void BlockScreen()
		{
			_screenBlocker.UnfadeAsync().Forget();
		}

		public void UnblockScreen()
		{
			_screenBlocker.FadeAsync().Forget();
		}


		[SerializeField] private string _debugPopupMessage;
		[Button] private void Debug_SetFeedScreenView() => SetScreenView(EScreenViewKey.Feed);
		[Button] private void Debug_SetSettingsScreenView() => SetScreenView(EScreenViewKey.Settings);
		[Button] private void Debug_SetExternalStoragesScreenView() => SetScreenView(EScreenViewKey.ExternalStorages);
	}
}
