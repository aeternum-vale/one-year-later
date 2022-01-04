using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI.Interfaces;
using OneYearLater.UI.Popups;
using OneYearLater.UI.Views.ScreenViews;
using UnityEngine;
using Zenject;

using static Utilities.Extensions;

namespace OneYearLater.UI
{

	public class ViewManager : MonoBehaviour, IViewManager
	{

		[Inject] private PopupManager _popupManager;
		[Inject] private IMobileInputHandler _mobileInputHandler;
		[Inject] private SideMenuView _sideMenu;


		[Header("Screen Views")]
		[SerializeField] private Transform _screenViewContainer;


		[Space(10)]
		[SerializeField] private CanvasGroupFader _screenBlocker;


		private Dictionary<EScreenViewKey, ScreenView> _screenViewKeyDictionary;
		private EScreenViewKey _currentScreenViewKey = EScreenViewKey.None;
		private CancellationTokenSource _screenViewChangeCTS;


		#region Unity Callbacks
		private void Awake()
		{
			_screenViewKeyDictionary =
				_screenViewContainer
				.GetComponentsInChildren<ScreenView>(true)
				.ToDictionary(sv => sv.Key);

			AddListeners();
		}

		#endregion

		private void AddListeners()
		{
			_mobileInputHandler.SwipeRight += OnSwipeRight;
			_mobileInputHandler.TapOnRightBorder += OnTapOnRightBorder;
		}

		private void OnSwipeRight(object sender, SwipeEventArgs args)
		{
			if (args.IsFromBorder && !_popupManager.IsAnyPopupActive) _sideMenu.Open(); //TODO maybe sidemenu must do it by itself
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
