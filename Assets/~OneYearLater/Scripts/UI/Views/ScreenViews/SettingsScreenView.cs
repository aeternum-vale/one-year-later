using System;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Views.ScreenViews
{
	public class SettingsScreenView : MonoBehaviour, ISettingsScreenView
	{
		public event EventHandler DeleteIdenticRecordsIntent;

		[SerializeField] private Button _deleteIdenticRecordsButton;

		private void Awake()
		{
			_deleteIdenticRecordsButton.onClick.AddListener(OnDeleteIdenticRecordsButtonClicked);
		}

		private void OnDeleteIdenticRecordsButtonClicked()
		{
			DeleteIdenticRecordsIntent?.Invoke(this, EventArgs.Empty);
		}

	}
}