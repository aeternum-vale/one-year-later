using System;
using System.Collections.Generic;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IExternalStoragesScreenView
	{
		void ProvideExternalStorageViewModels(IEnumerable<ExternalStorageViewModel> viewModels);
		void ChangeExternalStorageAppearance(EExternalStorageKey key, EExternalStorageAppearance appearance, string status = null);
		event EventHandler<EExternalStorageKey> ConnectToExternalStorageIntent;
		event EventHandler<EExternalStorageKey> DisconnectFromExternalStorageIntent;
		event EventHandler<EExternalStorageKey> SyncWithExternalStorageIntent;

	}
}