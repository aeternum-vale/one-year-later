using System;
using System.Collections.Generic;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IViewManager
	{
		void SetScreenView(EScreenViewKey screenViewKey);
		void ProvideExternalStorageViewModels(IEnumerable<ExternalStorageViewModel> viewModels);
		void ChangeExternalStorageAppearance(EExternalStorageKey key, EExternalStorageAppearance appearance, string status = null);
		void BlockScreen();
		void UnblockScreen();


		event EventHandler<EExternalStorageKey> ConnectToExternalStorageButtonClicked;
		event EventHandler<EExternalStorageKey> DisconnectFromExternalStorageButtonClicked;
		event EventHandler<EExternalStorageKey> SyncWithExternalStorageButtonClicked;
	}
}
