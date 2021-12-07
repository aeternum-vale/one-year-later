using System;
using System.Collections.Generic;
using System.Linq;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using OneYearLater.UI.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.UI.Views.ScreenViews
{
	[RequireComponent(typeof(ScreenView))]

	public class ExternalStoragesScreenView : MonoBehaviour, IScreenView, IExternalStoragesScreenView
	{
		public event EventHandler ConnectButtonClicked;
		public event EventHandler<EExternalStorageKey> ConnectToExternalStorageButtonClicked;
		public event EventHandler<EExternalStorageKey> DisconnectFromExternalStorageButtonClicked;
		public event EventHandler<EExternalStorageKey> SyncWithExternalStorageButtonClicked;

		[Inject] private IMobileInputHandler _mobileInputHandler;

		[SerializeField] private Transform _settingParametersContainer;
		[SerializeField] private ExternalStorageView _externalStorageViewPrefab;

		private ExternalStorageViewDataDict _externalStoragesViewData = new ExternalStorageViewDataDict();




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

			PopulateExternalStoragesList(_externalStoragesViewData);
		}

		public void ChangeExternalStorageAppearance(EExternalStorageKey key, EExternalStorageAppearance appearance, string status = null)
		{
			ExternalStorageView view = _externalStoragesViewData[key].view;
			view.ChangeAppearance(appearance, status);
		}

		public void PopulateExternalStoragesList(ExternalStorageViewDataDict dict)
		{
			foreach (var kvp in dict)
				kvp.Value.view.transform.SetParent(_settingParametersContainer);
		}

	}
}
