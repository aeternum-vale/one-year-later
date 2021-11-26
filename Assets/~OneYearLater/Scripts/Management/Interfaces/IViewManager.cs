using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IViewManager
	{
		UniTask DisplayDayFeedAsync(DateTime date, IEnumerable<BaseRecordViewModel> records);
		void SetIsDatePickingBlocked(bool isBlocked);
		void DisplayFeedLoading();
		void SetScreenView(EScreenViewKey screenViewKey);

		void ProvideExternalStorageViewModels(IEnumerable<ExternalStorageViewModel> viewModels);
		void ChangeExternalStorageAppearance(EExternalStorageKey key, EExternalStorageAppearance appearance, string status = null);

		event EventHandler<DateTime> DayChanged;

		event EventHandler<EExternalStorageKey> ConnectToExternalStorageButtonClicked;
		event EventHandler<EExternalStorageKey> DisconnectFromExternalStorageButtonClicked;
		event EventHandler<EExternalStorageKey> SyncWithExternalStorageButtonClicked;
	}
}
