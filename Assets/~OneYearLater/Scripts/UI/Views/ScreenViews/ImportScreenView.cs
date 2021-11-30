using System;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]
	public class ImportScreenView : MonoBehaviour, IScreenView, IImportScreen
	{
		public EScreenViewKey key => EScreenViewKey.Import;

		public event EventHandler ImportFromTextFileButtonClick;

		[SerializeField] private Button _importFromTxtButton;


		private void Awake()
		{
			_importFromTxtButton.onClick.AddListener(OnImportFromTxtButtonClick);
		}

		private void OnImportFromTxtButtonClick()
		{
			ImportFromTextFileButtonClick?.Invoke(this, EventArgs.Empty);
		}
	}
}