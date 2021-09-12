using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using OneYearLater.Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//using static Utilities.Extensions;

namespace OneYearLater.UI.Views
{
	[RequireComponent(typeof(SettingParameterView))]
	public class ExternalStorageView : MonoBehaviour
	{
		public event EventHandler ConnectButtonClicked;
		public event EventHandler SyncButtonClicked;

		[SerializeField] private Button _connectButton;
		[SerializeField] private Button _syncButton;
		[SerializeField] private TMP_Text _statusText;

		private EExternalStorageViewAppearanceState _currentViewState;
		private CanvasGroupFader _fader;
		private SettingParameterView _settingParameterView;
		private CancellationTokenSource _changeStateAnimationCTS;

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
			_syncButton.onClick.AddListener(OnSyncButtonClick);
		}

		private void OnConnectButtonClick()
		{
			if (_currentViewState == EExternalStorageViewAppearanceState.NotConnected)
				ConnectButtonClicked?.Invoke(this, EventArgs.Empty);
		}

		private void OnSyncButtonClick()
		{
			if (_currentViewState == EExternalStorageViewAppearanceState.Connected)
				SyncButtonClicked?.Invoke(this, EventArgs.Empty);
		}

		public void ChangeAppearance(EExternalStorageViewAppearanceState state, string status)
		{
			_currentViewState = state;

			_changeStateAnimationCTS?.Cancel();
			_changeStateAnimationCTS = new CancellationTokenSource();
			var token = _changeStateAnimationCTS.Token;

			switch (state)
			{
				case EExternalStorageViewAppearanceState.NotConnected:
					_fader.SetAlphaAsync(0.5f, token).Forget();
					_connectButton.gameObject.SetActive(true);
					_syncButton.gameObject.SetActive(false);

					break;
				case EExternalStorageViewAppearanceState.Connecting:
					_fader.SetAlphaAsync(0.7f, token).Forget();
					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(false);
					break;
				case EExternalStorageViewAppearanceState.Connected:
					_fader.SetAlphaAsync(1f, token).Forget();
					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(true);
					break;
				case EExternalStorageViewAppearanceState.Synchronizing:
					_fader.SetAlphaAsync(1f, token).Forget();
					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(true);

					_syncButton.transform.DORotate(new Vector3(0, 0, -360f), 1f, RotateMode.FastBeyond360)
						.SetEase(Ease.InOutSine);
						//.SetRelative();

					break;
				default:
					throw new Exception("invalid value of type EExteranalStorageViewAppearanceState");
			}

			_statusText.text = status;
		}

	}
}
