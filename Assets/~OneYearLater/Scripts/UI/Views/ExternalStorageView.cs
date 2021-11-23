using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using OneYearLater.Management;
using OneYearLater.UI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static OneYearLater.Management.Constants;

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

		private IMobileInputHandler _mobileInputHandler;
		public IMobileInputHandler MobileInputHandler { set => _mobileInputHandler = value; }

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

		private void Start()
		{
			_mobileInputHandler.LongTap += OnScreenLongTap;
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
			status ??= ExternalStorageAppearanceStatuses[appearance];

			_changeStateAnimationCTS?.Cancel();
			_changeStateAnimationCTS = new CancellationTokenSource();
			var token = _changeStateAnimationCTS.Token;

			switch (appearance)
			{
				case EExternalStorageAppearance.NotConnected:
					_fader.SetAlphaAsync(0.5f, token).Forget();

					_connectButton.gameObject.SetActive(true);
					_syncButton.gameObject.SetActive(false);
					_disconnectButton.gameObject.SetActive(false);
					break;

				case EExternalStorageAppearance.Connecting:
					_fader.SetAlphaAsync(0.7f, token).Forget();

					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(false);
					_disconnectButton.gameObject.SetActive(false);
					break;

				case EExternalStorageAppearance.Connected:
					_fader.SetAlphaAsync(1f, token).Forget();

					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(true);
					_disconnectButton.gameObject.SetActive(true);
					break;

				case EExternalStorageAppearance.Synchronizing:

					_fader.SetAlphaAsync(1f, token).Forget();

					_disconnectButton.gameObject.SetActive(false);
					_connectButton.gameObject.SetActive(false);
					_syncButton.gameObject.SetActive(true);

					_syncButton.transform
						.DORotate(new Vector3(0, 0, -360f), 0.6f, RotateMode.FastBeyond360)
						.SetEase(Ease.InOutSine);
					//.SetRelative();

					break;
				default:
					throw new Exception("invalid value of type EExternalStorageViewAppearanceState");
			}

			_statusText.text = status;
		}

		private void OnScreenLongTap(object sender, Vector2 tapScreenPos)
		{
			if (IsPointInRectTransform((RectTransform)transform, tapScreenPos))
				LongTap?.Invoke(this, EventArgs.Empty);
		}

		bool IsPointInRectTransform(RectTransform rt, Vector2 point)
		{
			return GetRectTransformBounds(rt).Contains(point);
		}

		public static Bounds GetRectTransformBounds(RectTransform transform)
		{
			Vector3[] worldCorners = new Vector3[4];
			transform.GetWorldCorners(worldCorners);
			Bounds bounds = new Bounds(worldCorners[0], Vector3.zero);
			for (int i = 1; i < 4; ++i)
				bounds.Encapsulate(worldCorners[i]);

			return bounds;
		}
	}
}
