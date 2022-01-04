using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using OneYearLater.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI.Views
{
	[RequireComponent(typeof(SettingParameterView))]
	public class ExternalStorageView : MonoBehaviour
	{
		public event EventHandler ConnectButtonClicked;
		public event EventHandler DisconnectButtonClicked;
		public event EventHandler SyncButtonClicked;

		[SerializeField] private Button _connectButton;
		[SerializeField] private Button _disconnectButton;
		[SerializeField] private Button _syncButton;
		[SerializeField] private TMP_Text _statusText;


		private EExternalStorageAppearance _currentViewAppearance;
		private CanvasGroupFader _fader;
		private SettingParameterView _settingParameterView;
		private CancellationTokenSource _changeStateAnimationCTS;

		public event EventHandler LongTap;

		public string Text
		{
			get => _settingParameterView.Text;
			set => _settingParameterView.Text = value;
		}

		private void Awake()
		{
			_settingParameterView = GetComponent<SettingParameterView>();
			_fader = GetComponent<CanvasGroupFader>();
			_fader.OwnFadeDuration = 1f;

			_connectButton.onClick.AddListener(OnConnectButtonClick);
			_disconnectButton.onClick.AddListener(OnDisconnectButtonClick);
			_syncButton.onClick.AddListener(OnSyncButtonClick);
		}

		private void OnConnectButtonClick()
		{
			if (_currentViewAppearance == EExternalStorageAppearance.NotConnected)
				ConnectButtonClicked?.Invoke(this, EventArgs.Empty);
		}

		private void OnDisconnectButtonClick()
		{
			if (_currentViewAppearance == EExternalStorageAppearance.Connected)
				DisconnectButtonClicked?.Invoke(this, EventArgs.Empty);
		}

		private void OnSyncButtonClick()
		{
			if (_currentViewAppearance == EExternalStorageAppearance.Connected)
				SyncButtonClicked?.Invoke(this, EventArgs.Empty);
		}

		public void ChangeAppearance(EExternalStorageAppearance appearance, string status = null)
		{
			_currentViewAppearance = appearance;
			status ??= Management.Constants.ExternalStorageAppearanceDefaultStatuses[appearance];

			_changeStateAnimationCTS?.Cancel();
			_changeStateAnimationCTS = new CancellationTokenSource();
			var token = _changeStateAnimationCTS.Token;

			switch (appearance)
			{
				case EExternalStorageAppearance.NotConnected:
					_fader.SetAlphaAsync(0.2f, token).Forget();

					_connectButton.gameObject.SetActive(true);
					_syncButton.gameObject.SetActive(false);
					_disconnectButton.gameObject.SetActive(false);
					break;

				case EExternalStorageAppearance.Connecting:
					_fader.SetAlphaAsync(0.3f, token).Forget();

					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(false);
					_disconnectButton.gameObject.SetActive(false);
					break;

				case EExternalStorageAppearance.Connected:
					_fader.SetAlphaAsync(0.45f, token).Forget();

					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(true);
					_disconnectButton.gameObject.SetActive(true);
					break;

				case EExternalStorageAppearance.Synchronizing:

					_fader.SetAlphaAsync(0.45f, token).Forget();

					_disconnectButton.gameObject.SetActive(false);
					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(true);

					_syncButton.transform
						.DORotate(new Vector3(0, 0, -360f), 1f, RotateMode.FastBeyond360)
						.SetEase(Ease.InOutSine);
					break;

				case EExternalStorageAppearance.Error:
					_fader.SetAlphaAsync(0.45f, token).Forget();

					_disconnectButton.gameObject.SetActive(false);
					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(false);
					break;

				default:
					throw new Exception($"invalid value of type {nameof(EExternalStorageAppearance)}");
			}

			_statusText.text = status;
		}


	}
}
