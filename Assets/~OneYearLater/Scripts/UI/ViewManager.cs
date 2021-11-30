using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using OneYearLater.UI.Interfaces;
using OneYearLater.UI.Popups;
using OneYearLater.UI.Views;
using OneYearLater.UI.Views.ScreenViews;
using UnityEngine;
using Zenject;

using static Utilities.Extensions;

namespace OneYearLater.UI
{

	public class ViewManager : MonoBehaviour, IViewManager
	{
		public event EventHandler<EExternalStorageKey> ConnectToExternalStorageButtonClicked;
		public event EventHandler<EExternalStorageKey> DisconnectFromExternalStorageButtonClicked;
		public event EventHandler<EExternalStorageKey> SyncWithExternalStorageButtonClicked;

		[Inject] private IMobileInputHandler _mobileInputHandler;

		[Header("Screen Views")]
		[SerializeField] private Transform _sceenViewContainer;

		[Inject] private FeedScreenView _feedScreenView;
		[Inject] private ImportScreenView _importScreenView;
		[SerializeField] private ExternalStoragesScreenView _externalStoragesScreenView;

		[Space(10)]
		[SerializeField] private ExternalStorageView _externalStorageViewPrefab;
		[SerializeField] private DiaryRecordView _diaryRecordViewPrefab;
		[SerializeField] private SideMenu _sideMenu;
		[SerializeField] private CanvasGroupFader _screenBlocker;
		[SerializeField] private PopupManager _popupManager;


		private Dictionary<EScreenViewKey, ScreenView> _screenViewKeyDictionary;
		private ExternalStorageViewDataDict _externalStoragesViewData
			= new ExternalStorageViewDataDict();
		private EScreenViewKey _currentScreenViewKey = EScreenViewKey.None;
		private CancellationTokenSource _screenViewChangeCTS;




		#region Unity Callbacks
		private void Awake()
		{
			_screenViewKeyDictionary = 
				_sceenViewContainer
				.GetComponentsInChildren<ScreenView>()
				.ToDictionary(sv => sv.Key);

			AddListeners();
		}

		private void Start()
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> _feedScreenView={_feedScreenView}");
			SetScreenView(EScreenViewKey.Feed);
		}

		#endregion

		private void AddListeners()
		{
			_mobileInputHandler.SwipeRight += OnSwipeRight;
			_mobileInputHandler.TapOnRightBorder += OnTapOnRightBorder;

			_sideMenu.ScreenViewButtonClick += (s, key) =>
			{
				SetScreenView(key);
				_sideMenu.Close();
			};
		}

		public void ProvideExternalStorageViewModels(IEnumerable<ExternalStorageViewModel> viewModels)
		{
			viewModels.ToList().ForEach(vm =>
			{
				ExternalStorageView view = Instantiate(_externalStorageViewPrefab);
				view.MobileInputHandler = _mobileInputHandler;
				view.Text = vm.name;
				_externalStoragesViewData.Add(
					vm.key,
					new ExternalStorageViewData()
					{
						view = view,
						viewModel = vm
					});

				view.ConnectButtonClicked += (s, a) =>
					ConnectToExternalStorageButtonClicked?.Invoke(this, vm.key);

				view.DisconnectButtonClicked += (s, a) =>
					DisconnectFromExternalStorageButtonClicked?.Invoke(this, vm.key);

				view.SyncButtonClicked += (s, a) =>
					SyncWithExternalStorageButtonClicked?.Invoke(this, vm.key);

				view.ChangeAppearance(EExternalStorageAppearance.NotConnected);
			});

			_externalStoragesScreenView.PopulateExternalStoragesList(_externalStoragesViewData);
		}

		public void ChangeExternalStorageAppearance(EExternalStorageKey key, EExternalStorageAppearance appearance, string status = null)
		{
			ExternalStorageView view = _externalStoragesViewData[key].view;
			view.ChangeAppearance(appearance, status);
		}

		private void OnSwipeRight(object sender, bool fromBorder)
		{
			if (fromBorder && !_popupManager.IsAnyPopupActive) _sideMenu.Open();
		}

		private void OnTapOnRightBorder(object sender, EventArgs args)
		{
			if (!_popupManager.IsAnyPopupActive)
				_sideMenu.Close();
		}

		public void SetScreenView(EScreenViewKey screenViewKey)
		{
			_screenViewChangeCTS?.Cancel();
			_screenViewChangeCTS = new CancellationTokenSource();
			var token = _screenViewChangeCTS.Token;

			foreach (var entry in _screenViewKeyDictionary)
				if (entry.Key != screenViewKey && entry.Value.gameObject.activeSelf)
					entry.Value.FadeAsync(token).Forget();

			_screenViewKeyDictionary[screenViewKey].UnfadeAsync(token).Forget();

			_currentScreenViewKey = screenViewKey;
		}


		public void BlockScreen()
		{
			_screenBlocker.UnfadeAsync().Forget();
		}

		public void UnblockScreen()
		{
			_screenBlocker.FadeAsync().Forget();
		}

	}
}
