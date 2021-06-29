using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedView : MonoBehaviour
{
	[SerializeField] private Transform _recordsContainer;

	[SerializeField] private Button _nextDayButton;
	[SerializeField] private Button _prevDayButton;
	[SerializeField] private Button _nextYearButton;
	[SerializeField] private Button _prevYearButton;

	[SerializeField] private TextMeshProUGUI _dateText;
	[SerializeField] private TextMeshProUGUI _loadingText;

	public event EventHandler<DateTime> DayChanged;




	private DateTime _visibleDate;

	public Transform RecordsContainer { get => _recordsContainer; set => _recordsContainer = value; }

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

	public void SetIsLoadingTextActive(bool isActive)
	{
		_loadingText.gameObject.SetActive(isActive);
	}
}
