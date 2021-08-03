using System.Threading;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Management;
using NaughtyAttributes;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UnityEngine;
using static Utilities.Extensions;

namespace OneYearLater.UI
{

	public class ViewManager : MonoBehaviour, IViewManager
	{
		[SerializeField] private ViewSPair[] _viewArray;
		private Dictionary<EScreenViewKey, ScreenView> _viewDictionary;


		[SerializeField] private FeedView _feedView;
		[SerializeField] private DiaryRecordView _diaryRecordViewPrefab;

		public event EventHandler<DateTime> DayChanged;
		public event EventHandler<string> XMLFilePicked;

		private EScreenViewKey _currentScreenViewKey = EScreenViewKey.None;

		private void Awake()
		{
			_viewArray.ToDictionary(out _viewDictionary);

			_feedView.DayChanged += OnFeedViewDayChanged;
		}

		private void Start() {
			SetScreenView(EScreenViewKey.Feed);
		}


		private CancellationTokenSource _screenViewChangeCTS;
		private void SetScreenView(EScreenViewKey screenViewKey)
		{
			_screenViewChangeCTS?.Cancel();
			_screenViewChangeCTS = new CancellationTokenSource();
			var token = _screenViewChangeCTS.Token;

			foreach (var entry in _viewDictionary)
				if (entry.Key != screenViewKey && entry.Value.gameObject.activeSelf)
					entry.Value.FadeAsync(token).Forget();
			
			_viewDictionary[screenViewKey].UnfadeAsync(token).Forget();

			_currentScreenViewKey = screenViewKey;
		}

		[Button] private void DebugSetFeedScreenView() => SetScreenView(EScreenViewKey.Feed);
		[Button] private void DebugSetSettingsScreenView() => SetScreenView(EScreenViewKey.Settings);
		[Button] private void DebugSetExternalStoragesScreenView() => SetScreenView(EScreenViewKey.ExternalStorages);
		

		private void OnFeedViewDayChanged(object sender, DateTime date)
		{
			DayChanged?.Invoke(this, date);
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
						case ERecord.Diary:

							DiaryRecordView v = Instantiate<DiaryRecordView>(_diaryRecordViewPrefab);
							DiaryRecordViewModel vm = (DiaryRecordViewModel)record;
							v.TimeText = vm.DateTime.ToString("HH:mm");
							v.ContentText = vm.Text;
							//LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)v.transform);
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
	}
}
