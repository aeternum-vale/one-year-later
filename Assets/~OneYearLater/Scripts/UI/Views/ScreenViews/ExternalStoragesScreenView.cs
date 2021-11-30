using System;
using System.Collections.Generic;
using OneYearLater.UI.Interfaces;
using UnityEngine;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]

	public class ExternalStoragesScreenView : MonoBehaviour, IScreenView
	{
		public event EventHandler ConnectButtonClicked;

		[SerializeField] private Transform _settingParametersContainer;
		public void PopulateExternalStoragesList(ExternalStorageViewDataDict dict)
		{
			foreach (var kvp in dict)
			{
				//kvp.ConnectButtonClicked += (s, a) => ConnectButtonClicked?.Invoke(this, EventArgs.Empty);

				kvp.Value.view.transform.SetParent(_settingParametersContainer);
			}
		}
	}
}
