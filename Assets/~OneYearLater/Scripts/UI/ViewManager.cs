using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using OneYearLater.UI.Popups;
using OneYearLater.UI.Views;
using OneYearLater.UI.Views.ScreenViews;
using UnityEngine;
using Zenject;

using static Utilities.Extensions;
using OneYearLater.UI.Interfaces;

namespace OneYearLater.UI
{

	public class ViewManager : MonoBehaviour, IViewManager
	{
		public event EventHandler<DateTime> DayChanged;
		public event EventHandler<EScreenViewKey> ScreenViewChanged;
		public event EventHandler<EExternalStorageKey> ConnectToExternalStorageButtonClicked;
		public event EventHandler<EExternalStorageKey> SynchronizeWithExternalStorageButtonClicked;

		[Inject] private IMobileInputHandler _mobileInputHandler;

		[SerializeField] private ScreenViewSPair[] _screenViewArray;
		private Dictionary<EScreenViewKey, ScreenView> _screenViewDictionary;


		[SerializeField] private ExternalStorageView _externalStorageViewPrefab;
		private ExternalStorageViewDataDict _externalStoragesViewData
			= new ExternalStorageViewDataDict();

		private FeedScreenView _feedView;
		private ExternalStoragesScreenView _externalStoragesScreenView;

		[SerializeField] private DiaryRecordView _diaryRecordViewPrefab;
		[SerializeField] private PopupManager _popupManager;

		[SerializeField] private SideMenu _sideMenu;


		private EScreenViewKey _currentScreenViewKey = EScreenViewKey.None;
		private CancellationTokenSource _screenViewChangeCTS;


		#region Unity Callbacks
		private void Awake()
		{
			_screenViewArray.ToDictionary(out _screenViewDictionary);

			_feedView = _screenViewDictionary[EScreenViewKey.Feed].GetComponent<FeedScreenView>();
			_externalStoragesScreenView =
				_screenViewDictionary[EScreenViewKey.ExternalStorages].GetComponent<ExternalStoragesScreenView>();

			_feedView.DayChanged += OnFeedViewDayChanged;

			_mobileInputHandler.SwipeRight += OnSwipeRight;
			_mobileInputHandler.TapOnRightBorder += OnTapOnRightBorder;

			_sideMenu.FeedButtonClick += (s, a) =>
			{
				SetScreenView(EScreenViewKey.Feed);
				_sideMenu.Close();
			};

			_sideMenu.ExternalStoragesButtonClick += (s, a) =>
			{
				SetScreenView(EScreenViewKey.ExternalStorages);
				_sideMenu.Close();
			};

			_externalStoragesScreenView.ConnectButtonClicked += (s, a) => _popupManager.ShowMessagePopupAsync("Hello World!", "hi!");
		}

		private void Start()
		{
			SetScreenView(EScreenViewKey.Feed);
		}
		#endregion

		public void ProvideExternalStorageViewModels(ExternalStorageViewModel[] vmArray, EExternalStorageViewAppearanceState defaultState, string defaultStatus)
		{
			vmArray.ToList().ForEach(vm =>
			{
				ExternalStorageView view = Instantiate(_externalStorageViewPrefab);
				view.Text = vm.name;
				_externalStoragesViewData.Add(
					vm.key,
					new ExternalStorageViewData()
					{
						view = view,
						viewModel = vm
					});

				view.ConnectButtonClicked += (s, a) => ConnectToExternalStorageButtonClicked?.Invoke(this, vm.key);
				view.SyncButtonClicked += (s, a) => SynchronizeWithExternalStorageButtonClicked?.Invoke(this, vm.key);

				view.ChangeAppearance(defaultState, defaultStatus);
			});

			_externalStoragesScreenView.PopulateExternalStoragesList(_externalStoragesViewData);
		}

		public void ChangeExternalStorageViewAppearance(EExternalStorageKey key, EExternalStorageViewAppearanceState state, string status)
		{
			ExternalStorageView view = _externalStoragesViewData[key].view;
			view.ChangeAppearance(state, status);
		}

		private void OnSwipeRight(object sender, bool fromBorder)
		{
			if (fromBorder) _sideMenu.Open();
		}

		private void OnTapOnRightBorder(object sender, EventArgs args)
		{
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

		private void SetScreenView(EScreenViewKey screenViewKey)
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

		public UniTask<string> ShowPromptPopupAsync(string messageText, string okButtonText, string placeholderText)
		{
			return _popupManager.ShowPromptPopupAsync(messageText, okButtonText, placeholderText);
		}

		[SerializeField] private string _debugPopupMessage;
		[Button] private void Debug_SetFeedScreenView() => SetScreenView(EScreenViewKey.Feed);
		[Button] private void Debug_SetSettingsScreenView() => SetScreenView(EScreenViewKey.Settings);
		[Button] private void Debug_SetExternalStoragesScreenView() => SetScreenView(EScreenViewKey.ExternalStorages);
		[Button] private void Debug_ShowMessagePopup() => _popupManager.ShowMessagePopupAsync(_debugPopupMessage).Forget();
		[Button] private void Debug_ShowPromptPopup() => _popupManager.ShowPromptPopupAsync(_debugPopupMessage).ContinueWith<string>((value) => Debug.Log(value)).Forget();

	}
}
