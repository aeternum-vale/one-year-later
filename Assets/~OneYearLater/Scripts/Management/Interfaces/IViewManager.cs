﻿using System;
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

		void ProvideExternalStorageViewModels(ExternalStorageViewModel[] array, EExternalStorageAppearance defaultState, string defaultStatus);
		void ChangeExternalStorageAppearance(EExternalStorageKey key, EExternalStorageAppearance state, string status);

		UniTask<string> ShowPromptPopupAsync(string messageText, string okButtonText, string placeholderText);

		event EventHandler<DateTime> DayChanged;
		event EventHandler<EScreenViewKey> ScreenViewChanged;

		event EventHandler<EExternalStorageKey> ConnectToExternalStorageButtonClicked;
		event EventHandler<EExternalStorageKey> SyncWithExternalStorageButtonClicked;
	}
}
