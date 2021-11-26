using System;
using System.Collections.Generic;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using OneYearLater.UI.Popups;
using OneYearLater.UI.Views;
using OneYearLater.UI.Views.ScreenViews;
using Utilities;

namespace OneYearLater.UI
{
	public static class Constants
	{
		public const float ScreenViewFadeDuration = 0.5f;
		public const float PopupAppearDuration = 0.3f;
		public const float PopupBackgroundFadeDuration = 0.2f;
	}

	public enum EPopupKey
	{
		None = 0,
		Message,
		Alert,
		Confirm,
		Prompt
	}

	[Serializable]
	public class ScreenViewSPair : SerializableKeyValuePair<EScreenViewKey, ScreenView> { }

	[Serializable]
	public class PopupSPair : SerializableKeyValuePair<EPopupKey, Popup> { }

	public struct ExternalStorageViewData
	{
		public ExternalStorageView view;
		public ExternalStorageViewModel viewModel;
	}

	public class ExternalStorageViewDataDict : Dictionary<EExternalStorageKey, ExternalStorageViewData> { };
}
