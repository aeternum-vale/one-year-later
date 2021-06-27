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
		[SerializeField] private MainView _mainView;

		[SerializeField] private DiaryRecordView _diaryRecordViewPrefab;
		[SerializeField] private RectTransform _scrollViewContent;

		public event EventHandler<DateTime> DayChanged;
		public event EventHandler<string> XMLFilePicked;

		public void DisplayDate(DateTime date)
		{
			_mainView.SetDate(date);
		}

		public UniTask DisplayDayFeedAsync(IEnumerable<BaseRecordViewModel> records)
		{
			_mainView.SetIsLoadingTextActive(false);

			float top = 0f;

			foreach (var record in records)
			{
				switch (record.Type)
				{
					case ERecord.Diary:

						DiaryRecordView v = Instantiate<DiaryRecordView>(_diaryRecordViewPrefab, _mainView.RecordsContainer);
						DiaryRecordViewModel vm = (DiaryRecordViewModel)record;
						v.TimeText = vm.DateTime.ToString("hh:mm");
						v.ContentText = vm.Text;

						var rect = v.GetComponent<RectTransform>().rect;

						v.transform.position -= new Vector3(0, top);
						top += rect.height;
						Debug.Log(top);

						break;
					default:
						throw new Exception("invalid record type");
				}
			}


			var recordsContainerRT = _mainView.RecordsContainer.GetComponent<RectTransform>();
			recordsContainerRT.sizeDelta = new Vector2(recordsContainerRT.sizeDelta.x, top);
			_scrollViewContent.sizeDelta = new Vector2(_scrollViewContent.sizeDelta.x, top);

			return UniTask.CompletedTask;
		}

		public void DisplayFeedLoading()
		{
			_mainView.SetIsLoadingTextActive(true);
		}

		public void SetIsDatePickingBlocked(bool isBlocked)
		{
			_mainView.SetIsDatePickingBlocked(isBlocked);
		}
	}
}
