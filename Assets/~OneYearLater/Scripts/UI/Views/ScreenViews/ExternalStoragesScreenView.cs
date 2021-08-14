using System.Collections.Generic;
using UnityEngine;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]

	public class ExternalStoragesScreenView : MonoBehaviour
	{
		[SerializeField] private Transform _settingParametersContainer;
		public void PopulateExternalStoragesList(IEnumerable<ExternalStorageSettingParameterView> settingParameterViews)
		{
			foreach (var item in settingParameterViews)
				item.transform.SetParent(_settingParametersContainer);
		}
	}
}
