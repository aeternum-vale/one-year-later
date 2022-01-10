using System;
using OneYearLater.Management.Interfaces;
using UnityEngine;

namespace OneYearLater.Management.Controllers
{
	public class SettingsScreenController
	{
		private ISettingsScreenView _view;

		public SettingsScreenController(ISettingsScreenView view)
		{
			_view = view;

			_view.DeleteIdenticRecordsIntent += OnDeleteIdenticRecordsIntent;
		}

		private void OnDeleteIdenticRecordsIntent(object sender, EventArgs args)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnDeleteIdenticRecordsIntent");
		}
	}
}