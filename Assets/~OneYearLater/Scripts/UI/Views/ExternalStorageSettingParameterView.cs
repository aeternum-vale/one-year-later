using System;
using UnityEngine;
using UnityEngine.UI;


namespace OneYearLater.UI.Views
{
	[RequireComponent(typeof(SettingParameterView))]
	public class ExternalStorageSettingParameterView : MonoBehaviour
	{
		public event EventHandler ConnectButtonClicked;
		[SerializeField] private Button _connectButton;

		private SettingParameterView _settingParameterView;

		public string Text
		{
			get => _settingParameterView.Text;
			set => _settingParameterView.Text = value;
		}

		private void Awake()
		{
			_settingParameterView = GetComponent<SettingParameterView>();
			
			_connectButton.onClick.AddListener(OnConnectButtonClick);
		}

		private void OnConnectButtonClick()
		{
			ConnectButtonClicked?.Invoke(this, EventArgs.Empty);
		}

	}
}
