using System.Numerics;
using System;
using Vector2 = UnityEngine.Vector2;

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

		/// <summary>Vector2 = screen position in pixels, where 0,0 = bottom left</summary>
		event EventHandler<Vector2> LongTap;
	}
}
