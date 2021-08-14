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

namespace OneYearLater.UI
{

	public class ViewManager : MonoBehaviour, IViewManager
	{
		public event EventHandler<DateTime> DayChanged;

		[Inject] private IExternalStorage[] _externalStorages;

		[SerializeField] private ScreenViewSPair[] _screenViewArray;
		private Dictionary<EScreenViewKey, ScreenView> _screenViewDictionary;

		[SerializeField] private ExternalStorageSettingParameterView _externalStorageSettingParameterViewPrefab;

		private FeedScreenView _feedView;
		private ExternalStoragesScreenView _externalStoragesScreenView;

		[SerializeField] private DiaryRecordView _diaryRecordViewPrefab;
		[SerializeField] private PopupManager _popupManager;

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
		}

		private void Start()
		{
			SetScreenView(EScreenViewKey.Feed);
			PopulateExternalStorageSettingParameterView();
		}
		#endregion

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

		private void PopulateExternalStorageSettingParameterView()
		{
			var settingParameterViews = new List<ExternalStorageSettingParameterView>();

			_externalStorages.ToList().ForEach(es =>
			{
				var spView = Instantiate(_externalStorageSettingParameterViewPrefab);
				spView.Text = es.Name;

				settingParameterViews.Add(spView);
			});

			_externalStoragesScreenView.PopulateExternalStoragesList(settingParameterViews);
		}

		[SerializeField] private string _debugPopupMessage;
		[Button] private void Debug_SetFeedScreenView() => SetScreenView(EScreenViewKey.Feed);
		[Button] private void Debug_SetSettingsScreenView() => SetScreenView(EScreenViewKey.Settings);
		[Button] private void Debug_SetExternalStoragesScreenView() => SetScreenView(EScreenViewKey.ExternalStorages);
		[Button] private void Debug_ShowMessagePopup() => _popupManager.ShowMessagePopupAsync(_debugPopupMessage).Forget();
		[Button] private void Debug_ShowPromptPopup() => _popupManager.ShowPromptPopupAsync(_debugPopupMessage).ContinueWith<string>((value) => Debug.Log(value)).Forget();
	}
}
