using System.Numerics;
using System;
using Vector2 = UnityEngine.Vector2;
using UnityEngine;

namespace OneYearLater.UI.Interfaces
{

	public class SwipeEventArgs : EventArgs
	{
		public bool IsFromBorder { get; set; }
	}

	public interface IMobileInputHandler
	{
		float SwipeBorderSize { get; set; }
		float TapBorderSize { get; set; }

		event EventHandler<SwipeEventArgs> SwipeLeft;
		event EventHandler<SwipeEventArgs> SwipeRight;
		event EventHandler TapOnRightBorder;
		event EventHandler<Vector2> LongTap;

		void SubscribeToLongTap(RectTransform rectTransform, Action onLongTap);
	}


}
