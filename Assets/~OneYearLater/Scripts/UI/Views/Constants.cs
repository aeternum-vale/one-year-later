using System.Collections.Generic;
using System;
using Utilities;

namespace OneYearLater.UI
{
	public static class Constants
	{
		public const float ScreenViewFadeDuration = 0.5f;
	}

	[Serializable]
	public enum EScreenViewKey
	{
		None,
		Feed,
		Settings,
		ExternalStorages
	}

	[Serializable]
	public class ViewSPair : SerializableKeyValuePair<EScreenViewKey, ScreenView> { }

}
