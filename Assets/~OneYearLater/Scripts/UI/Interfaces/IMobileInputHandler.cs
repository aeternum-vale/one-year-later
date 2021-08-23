using System;

namespace OneYearLater.UI.Interfaces
{
	public interface IMobileInputHandler
	{
		float SwipeBorderSize { get; set; }
		float TapBorderSize { get; set; }


		/// <summary>bool = is swipe made from border</summary>
		event EventHandler<bool> SwipeLeft;

		/// <summary>bool = is swipe made from border</summary>
		event EventHandler<bool> SwipeRight;

		event EventHandler TapOnRightBorder;
	}
}
