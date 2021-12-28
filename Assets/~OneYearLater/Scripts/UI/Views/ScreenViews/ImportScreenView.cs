using System;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]
	public class ImportScreenView : MonoBehaviour, IScreenView, IImportScreenView
	{
		public EScreenViewKey key => EScreenViewKey.Import;

		public event EventHandler ImportFromTextFileIntent;

		[SerializeField] private Button _importFromTxtButton;


		private void Awake()
		{
			_importFromTxtButton.onClick.AddListener(OnImportFromTxtButtonClick);
		}

		private void OnImportFromTxtButtonClick()
		{
			ImportFromTextFileIntent?.Invoke(this, EventArgs.Empty);
		}
	}
}