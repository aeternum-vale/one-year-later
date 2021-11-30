using System;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]
	public class ImportScreenView : MonoBehaviour
	{
		public event EventHandler ImportFromTxtButtonClick;

		[SerializeField] private Button _importFromTxtButton;

		private void Awake()
		{
			_importFromTxtButton.onClick.AddListener(OnImportFromTxtButtonClick);
		}

		private void OnImportFromTxtButtonClick()
		{
			ImportFromTxtButtonClick?.Invoke(this, EventArgs.Empty);
		}
	}
}