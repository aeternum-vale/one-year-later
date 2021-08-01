using System.Collections.Generic;
using System;
using Utilities;

namespace OneYearLater.UI
{
	public static class Constants
	{
		public const float ScreenViewFadeDuration = 1f;
	}

	[Serializable]
	public enum EView
	{
		None,
		Feed,
		Settings
	}

	[Serializable]
	public class ViewSPair : SerializableKeyValuePair<EView, ScreenView> { }

}
