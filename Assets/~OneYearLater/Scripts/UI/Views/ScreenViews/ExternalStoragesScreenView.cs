using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]

	public class ExternalStoragesScreenView : MonoBehaviour
	{
		public event EventHandler ConnectButtonClicked;

		[SerializeField] private Transform _settingParametersContainer;
		public void PopulateExternalStoragesList(IEnumerable<ExternalStorageSettingParameterView> settingParameterViews)
		{
			foreach (var item in settingParameterViews)
			{
				item.ConnectButtonClicked += (s, a) => ConnectButtonClicked?.Invoke(this, EventArgs.Empty);
				item.transform.SetParent(_settingParametersContainer);
			}
		}
	}
}
