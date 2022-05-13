using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using OneYearLater.UI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Zenject;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]
	public class FeedScreenView : MonoBehaviour, IScreenView, IFeedScreenView
	{
		[Inject] private IMobileInputHandler _mobileInputHandler;

		[SerializeField] private DiaryRecordView _diaryRecordViewPrefab;

		[SerializeField] private RectTransform _recordsContainer;
		[SerializeField] private RectTransform _scrollViewContent;
		[SerializeField] private ScrollRect _feedScrollRect;

		[SerializeField] private Button _nextDayButton;
		[SerializeField] private Button _prevDayButton;
		[SerializeField] private Button _nextMonthButton;
		[SerializeField] private Button _prevMonthButton;
		[SerializeField] private Button _nextYearButton;
		[SerializeField] private Button _prevYearButton;

		[SerializeField] private Button _dateButton;
		[SerializeField] private TextMeshProUGUI _dateText;
		[SerializeField] private GameObject _loadingImage;
		[SerializeField] private GameObject _noRecordsMessage;
		[SerializeField] private Button _addRecordButton;

		public event EventHandler<DateTime> DayChangeIntent;
		public event EventHandler AddRecordIntent;
		public event EventHandler<string> EditRecordIntent;
		private DateTime _visibleDate;


		private void Awake()
		{
			AddListeners();
		}

		private void AddListeners()
		{
			_mobileInputHandler.SwipeLeft += OnSwipeLeft;
			_mobileInputHandler.SwipeRight += OnSwipeRight;

			_nextDayButton.onClick.AddListener(NextDayButtonClicked);
			_prevDayButton.onClick.AddListener(PrevDayButtonClicked);
			_nextMonthButton.onClick.AddListener(NextMonthButtonClicked);
			_prevMonthButton.onClick.AddListener(PrevMonthButtonClicked);
			_nextYearButton.onClick.AddListener(NextYearButtonClicked);
			_prevYearButton.onClick.AddListener(PrevYearButtonClicked);

			_dateButton.onClick.AddListener(OnDateButtonClicked);

			_addRecordButton.onClick.AddListener(OnAddRecordButtonClicked);
		}

		public async UniTask DisplayDayFeedAsync(DateTime date, IEnumerable<BaseRecordViewModel> records)
		{
			SetDate(date);

			SetIsNoRecordsMessageActive(false);
			SetIsLoadingImageActive(false);
			ClearRecordsContainer();

			if (records.IsAny())
			{
				SetIsLoadingImageActive(true);
				List<GameObject> recordGameObjects = new List<GameObject>();
				DiaryRecordView diaryView;
				foreach (var record in records)
				{
					switch (record.Type)
					{
						case ERecordType.Diary:
							diaryView = Instantiate(_diaryRecordViewPrefab);
							var diaryVM = (DiaryRecordViewModel)record;

							diaryView.Hash = diaryVM.Hash;
							diaryView.TimeText = diaryVM.RecordDateTime.ToString("HH:mm");
							diaryView.ContentText = diaryVM.Text;

							diaryView.LongTap += (s, a) => EditRecordIntent?.Invoke(this, diaryVM.Hash);

							recordGameObjects.Add(diaryView.gameObject);
							break;

						case ERecordType.Message:
							diaryView = Instantiate(_diaryRecordViewPrefab);
							var messageVM = (MessageRecordViewModel)record;

							diaryView.Hash = messageVM.Hash;
							diaryView.TimeText = messageVM.RecordDateTime.ToString("HH:mm");

							string preposition = messageVM.IsFromUser ? "to" : "from";
							string contentText = $"<b>Message {preposition} <i>{messageVM.Conversationalist.Name}</i>:</b>\n\n{messageVM.MessageText}";

							diaryView.ContentText = contentText;

							diaryView.LongTap += (s, a) => EditRecordIntent?.Invoke(this, messageVM.Hash);

							recordGameObjects.Add(diaryView.gameObject);
							break;
						default:
							throw new Exception("invalid record type");
					}
				}
				await FillRecordsContainer(recordGameObjects);
				SetIsLoadingImageActive(false);
			}
			else
				SetIsNoRecordsMessageActive(true);
		}

		public void SetIsDatePickingBlocked(bool isBlocked)
		{
			bool interactable = !isBlocked;
			_nextDayButton.interactable = interactable;
			_prevDayButton.interactable = interactable;
			_nextMonthButton.interactable = interactable;
			_prevMonthButton.interactable = interactable;
			_nextYearButton.interactable = interactable;
			_prevYearButton.interactable = interactable;
		}

		public void DisplayThatFeedIsLoading()
		{
			SetIsNoRecordsMessageActive(false);
			ClearRecordsContainer();
			SetIsLoadingImageActive(true);
		}

		private void SetIsLoadingImageActive(bool isActive)
		{
			_loadingImage.gameObject.SetActive(isActive);
		}

		private void SetIsNoRecordsMessageActive(bool isActive)
		{
			_noRecordsMessage.gameObject.SetActive(isActive);
		}

		private void SetDate(DateTime date)
		{
			_visibleDate = date;
			_dateText.text = _visibleDate.ToString("dddd d MMMM, yyyy", CultureInfo.InvariantCulture);
		}

		private async UniTask FillRecordsContainer(IEnumerable<GameObject> records)
		{
			foreach (var record in records)
				record.transform.SetParent(_recordsContainer);

			await _recordsContainer.RebuildLayout();
			await _scrollViewContent.RebuildLayout();

			_feedScrollRect.verticalNormalizedPosition = 1;
		}

		private void ClearRecordsContainer()
		{
			foreach (Transform child in _recordsContainer)
				Destroy(child.gameObject);
		}

		private void OnSwipeLeft(object sender, SwipeEventArgs args)
		{
			if (!args.IsFromBorder)
				DayChangeIntent?.Invoke(this, _visibleDate.AddDays(1));
		}

		private void OnSwipeRight(object sender, SwipeEventArgs args)
		{
			if (!args.IsFromBorder)
				DayChangeIntent?.Invoke(this, _visibleDate.AddDays(-1));
		}

		private void NextYearButtonClicked() =>
			DayChangeIntent?.Invoke(this, _visibleDate.AddYears(1));

		private void PrevYearButtonClicked() =>
			DayChangeIntent?.Invoke(this, _visibleDate.AddYears(-1));

		private void NextMonthButtonClicked() =>
			DayChangeIntent?.Invoke(this, _visibleDate.AddMonths(1));

		private void PrevMonthButtonClicked() =>
			DayChangeIntent?.Invoke(this, _visibleDate.AddMonths(-1));

		private void PrevDayButtonClicked() =>
			DayChangeIntent?.Invoke(this, _visibleDate.AddDays(-1));

		private void NextDayButtonClicked() =>
			DayChangeIntent?.Invoke(this, _visibleDate.AddDays(1));


		private void OnDateButtonClicked() =>
			DayChangeIntent?.Invoke(this, DateTime.Today);
		

		private void OnAddRecordButtonClicked() =>
			AddRecordIntent?.Invoke(this, EventArgs.Empty);

	}
}
