using System.Collections.Generic;
using System;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]
	public class ImportScreenView : MonoBehaviour, IScreenView, IImportScreenView
	{
		public EScreenViewKey key => EScreenViewKey.Import;

		private Dictionary<EImportType, bool> _importProgressStatus = new Dictionary<EImportType, bool>()
		{
			[EImportType.DiaryFromTxt] = false,
			[EImportType.ConversationFromTxt] = false
		};

		private bool _isImportingAvailable;
		public bool IsImportingAllowed
		{
			get => _isImportingAvailable;
			set
			{
				_isImportingAvailable = value;
				_importDiaryFromTextFileButton.interactable =
					value && !_importProgressStatus[EImportType.DiaryFromTxt];
				_importConversationFromTextFileButton.interactable =
					value && !_importProgressStatus[EImportType.ConversationFromTxt];
			}
		}

		public event EventHandler<EImportType> ImportIntent;

		public bool IsImportInProgress(EImportType type)
		{
			return _importProgressStatus[type];
		}

		public void SetIsImportInProgress(EImportType type, bool isImportInProgress)
		{
			switch (type)
			{
				case EImportType.DiaryFromTxt:
					_importDiaryFromTextFileButtonText.text =
						isImportInProgress ?
						ImportDiaryFromTextFileButtonInProgressText :
						ImportDiaryFromTextFileButtonNormalText;

					_importDiaryFromTextFileButton.interactable = !isImportInProgress;
					break;
				case EImportType.ConversationFromTxt:
					_importConversationFromTextFileButtonText.text =
						isImportInProgress ?
						ImportConversationFromTextFileButtonInProgressText :
						ImportConversationFromTextFileButtonNormalText;

					_importConversationFromTextFileButton.interactable = !isImportInProgress;
					break;
			}

			_importProgressStatus[type] = isImportInProgress;
		}

		public void SetImportFileProgress(EImportType type, float value)
		{
			if (!_importProgressStatus[type]) return;

			int progressPercents = Mathf.RoundToInt(value * 100f);

			switch (type)
			{
				case EImportType.DiaryFromTxt:
					_importDiaryFromTextFileButtonText.text =
						$"{ImportDiaryFromTextFileButtonInProgressText} ({progressPercents}%)";
					break;
				case EImportType.ConversationFromTxt:
					_importConversationFromTextFileButtonText.text =
						$"{ImportConversationFromTextFileButtonInProgressText} ({progressPercents}%)";
					break;
			}

		}

		[SerializeField] private Button _importDiaryFromTextFileButton; //TODO put this to dict
		[SerializeField] private TMP_Text _importDiaryFromTextFileButtonText;
		[SerializeField] private Button _importConversationFromTextFileButton;
		[SerializeField] private TMP_Text _importConversationFromTextFileButtonText;

		private const string ImportDiaryFromTextFileButtonNormalText = "Import Diary from .txt";
		private const string ImportDiaryFromTextFileButtonInProgressText = "Importing Diary from .txt";
		private const string ImportConversationFromTextFileButtonNormalText = "Import Conversation from .txt";
		private const string ImportConversationFromTextFileButtonInProgressText = "Importing Conversation from .txt";


		private void Awake()
		{
			_importDiaryFromTextFileButton.onClick.AddListener(() => InvokeImportIntentEvent(EImportType.DiaryFromTxt));
			_importConversationFromTextFileButton.onClick.AddListener(() => InvokeImportIntentEvent(EImportType.ConversationFromTxt));
		}

		private void InvokeImportIntentEvent(EImportType t) => ImportIntent?.Invoke(this, t);
	}
}