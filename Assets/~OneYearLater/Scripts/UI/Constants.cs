using System;
using OneYearLater.UI.Popups;
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

	public enum EScreenViewKey
	{
		None = 0,
		Feed,
		Settings,
		ExternalStorages
	}

	public enum EPopupKey
	{
		None = 0,
		Message,
		Alert,
		Confirm,
		Promt
	}


	[Serializable]
	public class ScreenViewSPair : SerializableKeyValuePair<EScreenViewKey, ScreenView> { }

	[Serializable]
	public class PopupSPair : SerializableKeyValuePair<EPopupKey, Popup> { }
}
