using System;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
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
		private BaseRecordViewModel _editingRecordVM;

		//private string Text { get => _inputField.text; set => _inputField.text = value; }

		public BaseRecordViewModel EditingRecordViewModel { get => _editingRecordVM; set => SetEditingRecord(value); }


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
			_applyButton.onClick.AddListener(OnApplyButtonClick);
			_cancelButton.onClick.AddListener(() => CancelIntent?.Invoke(this, EventArgs.Empty));
			_deleteButton.onClick.AddListener(() => DeleteIntent?.Invoke(this, EventArgs.Empty));
		}

		private void SetEditingRecord(BaseRecordViewModel recordVM)
		{
			_editingRecordVM = recordVM;
			SetDate(recordVM.DateTime);

			switch (recordVM.Type)
			{
				case ERecordType.Diary:
					_inputField.text = ((DiaryRecordViewModel)recordVM).Text;
					break;
				case ERecordType.Message:
					_inputField.text = ((MessageRecordViewModel)recordVM).MessageText;
					break;
			}
		}

		private void SetDate(DateTime date)
		{
			_currentDate = date;
			_dateText.text = date.ToString("F");
		}

		private void OnApplyButtonClick()
		{
			
			switch (_editingRecordVM.Type)
			{
				case ERecordType.Diary:
					((DiaryRecordViewModel)_editingRecordVM).Text = _inputField.text;
					break;
				case ERecordType.Message:
					((MessageRecordViewModel)_editingRecordVM).MessageText = _inputField.text;
					break;
			}

			ApplyIntent?.Invoke(this, EventArgs.Empty);
		}

	}
}