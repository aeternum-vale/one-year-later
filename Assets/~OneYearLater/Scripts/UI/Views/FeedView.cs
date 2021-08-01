using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI
{
	public class FeedView : ScreenView
	{
		[SerializeField] private Transform _recordsContainer;

		[SerializeField] private Button _nextDayButton;
		[SerializeField] private Button _prevDayButton;
		[SerializeField] private Button _nextYearButton;
		[SerializeField] private Button _prevYearButton;

		[SerializeField] private TextMeshProUGUI _dateText;
		[SerializeField] private GameObject _loadingImage;
		[SerializeField] private GameObject _noRecordsMessage;
		[SerializeField] private ScrollRect _feedScrollView;
		[SerializeField] private RectTransform _scrollViewContent;

		public event EventHandler<DateTime> DayChanged;
		private DateTime _visibleDate;

		//[Header("Debug")]


		private void Awake()
		{
			AddListeners();
		}

		public void SetDate(DateTime date)
		{
			_visibleDate = date;
			_dateText.text = _visibleDate.ToString("dd.MM.yyyy");
		}

		private void AddListeners()
		{
			_nextDayButton.onClick.AddListener(NextDayButtonClicked);
			_prevDayButton.onClick.AddListener(PrevDayButtonClicked);
			_nextYearButton.onClick.AddListener(NextYearButtonClicked);
			_prevYearButton.onClick.AddListener(PrevYearButtonClicked);
		}

		public async UniTask DisplayRecords(IEnumerable<GameObject> records)
		{
			ClearRecordsContainer();

			foreach (var record in records)
				record.transform.SetParent(_recordsContainer);

			await UniTask.WaitForEndOfFrame();

			var feedViewLayoutGroup = _recordsContainer.GetComponent<LayoutGroup>();
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)feedViewLayoutGroup.transform);

			await UniTask.WaitForEndOfFrame();
			var contentLayoutGroup = _scrollViewContent.GetComponent<LayoutGroup>();
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentLayoutGroup.transform);

			_feedScrollView.verticalNormalizedPosition = 1;
		}

		public void ClearRecordsContainer()
		{
			foreach (Transform child in _recordsContainer)
				Destroy(child.gameObject);
		}


		private void NextYearButtonClicked()
		{
			DayChanged?.Invoke(this, _visibleDate.AddYears(1));
		}

		private void PrevYearButtonClicked()
		{
			DayChanged?.Invoke(this, _visibleDate.AddYears(-1));

		}

		private void PrevDayButtonClicked()
		{
			DayChanged?.Invoke(this, _visibleDate.AddDays(-1));

		}

		private void NextDayButtonClicked()
		{
			DayChanged?.Invoke(this, _visibleDate.AddDays(1));
		}

		public void SetIsDatePickingBlocked(bool isBlocked)
		{
			bool interactable = !isBlocked;
			_nextDayButton.interactable = interactable;
			_prevDayButton.interactable = interactable;
			_nextYearButton.interactable = interactable;
			_prevYearButton.interactable = interactable;
		}

		public void SetIsLoadingImageActive(bool isActive)
		{
			_loadingImage.gameObject.SetActive(isActive);
		}

		public void SetIsNoRecordsMessageActive(bool isActive)
		{
			_noRecordsMessage.gameObject.SetActive(isActive);
		}

	}
}
