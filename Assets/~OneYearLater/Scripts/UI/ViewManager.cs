using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UniRx.Async;
using UnityEngine;

namespace OneYearLater.UI
{
	public class ViewManager : MonoBehaviour, IViewManager
	{
		[SerializeField] private FeedView _feedView;

		[SerializeField] private DiaryRecordView _diaryRecordViewPrefab;
		[SerializeField] private RectTransform _scrollViewContent;

		public event EventHandler<DateTime> DayChanged;
		public event EventHandler<string> XMLFilePicked;

		private void Awake()
		{
			_feedView.DayChanged += OnFeedViewDayChanged;
		}

		private void OnFeedViewDayChanged(object sender, DateTime date)
		{
			DayChanged?.Invoke(this, date);
		}

		public void DisplayDate(DateTime date)
		{
			_feedView.SetDate(date);
		}

		public UniTask DisplayDayFeedAsync(IEnumerable<BaseRecordViewModel> records)
		{
			_feedView.SetIsLoadingImageActive(true);

			foreach (Transform child in _feedView.RecordsContainer)
				GameObject.Destroy(child.gameObject);

			foreach (var record in records)
			{
				switch (record.Type)
				{
					case ERecord.Diary:

						DiaryRecordView v = Instantiate<DiaryRecordView>(_diaryRecordViewPrefab, _feedView.RecordsContainer);
						DiaryRecordViewModel vm = (DiaryRecordViewModel)record;
						v.TimeText = vm.DateTime.ToString("hh:mm");
						v.ContentText = vm.Text;

						break;
					default:
						throw new Exception("invalid record type");
				}
			}

			_feedView.SetIsLoadingImageActive(false);

			return UniTask.CompletedTask;
		}

		public void DisplayFeedLoading()
		{
			_feedView.SetIsLoadingImageActive(true);
		}

		public void SetIsDatePickingBlocked(bool isBlocked)
		{
			_feedView.SetIsDatePickingBlocked(isBlocked);
		}
	}
}
