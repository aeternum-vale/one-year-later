using System;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Views.ScreenViews
{
	public class RecordEditorScreenView : MonoBehaviour, IScreenView, IRecordEditorScreenView
	{
		[SerializeField] TMP_Text _dateText;
		[SerializeField] private TMP_InputField _inputField;
		[SerializeField] private Button _applyButton;
		[SerializeField] private Button _cancelButton;
		[SerializeField] private Button _deleteButton;
		private DateTime _currentDate;

		public DateTime DateTime { get => _currentDate; set => SetDate(value); }
		public string Text { get => _inputField.text; set => _inputField.text = value; }

		public event EventHandler ApplyIntent;
		public event EventHandler CancelIntent;
		public event EventHandler DeleteIntent;


		private void Awake()
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> Awake");
			AddListeners();
		}

		private void AddListeners()
		{
			_applyButton.onClick.AddListener(() => ApplyIntent?.Invoke(this, EventArgs.Empty));
			_cancelButton.onClick.AddListener(() => CancelIntent?.Invoke(this, EventArgs.Empty));
			_deleteButton.onClick.AddListener(() => DeleteIntent?.Invoke(this, EventArgs.Empty));
		}

		private void SetDate(DateTime date)
		{
			_currentDate = date;
			_dateText.text = date.ToString("F");
		}
	}
}