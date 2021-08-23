using System;
using Lean.Touch;
using OneYearLater.UI.Interfaces;
using UnityEngine;

namespace OneYearLater.UI
{
	public class LeanTouchFacade : MonoBehaviour, IMobileInputHandler
	{

		[SerializeField] private LeanFingerSwipe _leanFingerSwipeLeft;
		[SerializeField] private LeanFingerSwipe _leanFingerSwipeRight;

		[SerializeField] private float _swipeBorderSize = 50f;
		[SerializeField] private float _tapBorderSize = 200f;

		public float SwipeBorderSize { get => _swipeBorderSize; set => _swipeBorderSize = value; }
		public float TapBorderSize { get => _tapBorderSize; set => _tapBorderSize = value; }


		/// <summary>bool = is swipe made from border</summary>
		public event EventHandler<bool> SwipeLeft;

		/// <summary>bool = is swipe made from border</summary>
		public event EventHandler<bool> SwipeRight;

		public event EventHandler TapOnRightBorder;


		private void Awake()
		{
			_leanFingerSwipeLeft.OnFinger.AddListener(OnSwipeLeft);
			_leanFingerSwipeRight.OnFinger.AddListener(OnSwipeRight);
			LeanTouch.OnFingerTap += OnTap;
		}

		private void OnSwipeLeft(LeanFinger leanFinger)
		{
			bool fromBorder = leanFinger.StartScreenPosition.x >= Screen.width - _swipeBorderSize;

			SwipeLeft?.Invoke(this, fromBorder);
		}

		private void OnSwipeRight(LeanFinger leanFinger)
		{
			bool fromBorder = leanFinger.StartScreenPosition.x <= _swipeBorderSize;

			SwipeRight?.Invoke(this, fromBorder);
		}

		private void OnTap(LeanFinger leanFinger)
		{
			if (leanFinger.LastScreenPosition.x >= Screen.width - _tapBorderSize)
				TapOnRightBorder?.Invoke(this, EventArgs.Empty);
		}

	}
}
