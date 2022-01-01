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

		public void SetIsImportFromTextFileInProgress(bool isInProgress)
		{
			_importFromTextFileButtonText.text =
				isInProgress ?
				ImportFromTextFileButtonInProgressText :
				ImportFromTextFileButtonNormalText;

			_importFromTextFileButton.interactable = !isInProgress;

		}

		public void SetImportFromTextFileProgressValue(float progress)
		{
			int progressPercents = Mathf.RoundToInt(progress * 100f);

			_importFromTextFileButtonText.text = $"{ImportFromTextFileButtonInProgressText} ({progressPercents}%)";
		}
	}
}