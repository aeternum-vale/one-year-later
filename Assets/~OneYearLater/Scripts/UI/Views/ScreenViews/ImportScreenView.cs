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

		public event EventHandler ImportFromTextFileIntent;

		public bool IsImportFromTextFileInProgress
		{
			get => _isImportFromTextFileInProgress;
			set => SetIsImportFromTextFileInProgress(value);
		}
		private bool _isImportFromTextFileInProgress;


		[SerializeField] private Button _importFromTextFileButton;
		[SerializeField] private TMP_Text _importFromTextFileButtonText;

		private const string ImportFromTextFileButtonNormalText = "Import from .txt";
		private const string ImportFromTextFileButtonInProgressText = "Importing from .txt";


		private void Awake()
		{
			_importFromTextFileButton.onClick.AddListener(OnImportFromTxtButtonClick);
		}

		private void OnImportFromTxtButtonClick()
		{
			ImportFromTextFileIntent?.Invoke(this, EventArgs.Empty);
		}

		private void SetIsImportFromTextFileInProgress(bool isInProgress)
		{
			_importFromTextFileButtonText.text =
				isInProgress ?
				ImportFromTextFileButtonInProgressText :
				ImportFromTextFileButtonNormalText;

			_importFromTextFileButton.interactable = !isInProgress;

			_isImportFromTextFileInProgress = isInProgress;
		}

		public void SetImportFromTextFileProgress(float value)
		{
			if (!_isImportFromTextFileInProgress) return;

			int progressPercents = Mathf.RoundToInt(value * 100f);
			_importFromTextFileButtonText.text = $"{ImportFromTextFileButtonInProgressText} ({progressPercents}%)";
		}

	}
}