using System;
using Lean.Touch;
using UnityEngine;

using static OneYearLater.UI.Utils;


namespace OneYearLater.UI
{
	public class Tapable : MonoBehaviour
	{
		[SerializeField] private float _longTapThreshold = 1f;

		public event EventHandler Tap;
		public event EventHandler LongTap;
		public event EventHandler DoubleTap;

		private void Awake()
		{
			LeanTouch.OnFingerTap += OnFingerTap;
			LeanTouch.OnFingerUp += OnFingerUp;
		}

		private void OnDestroy()
		{
			LeanTouch.OnFingerTap -= OnFingerTap;
			LeanTouch.OnFingerUp -= OnFingerUp;
		}

		private bool IsReadyToInvokeEvents(LeanFinger leanFinger)
		{
			if (!gameObject.activeInHierarchy) return false;

			RectTransform rt = (RectTransform)transform;

			if (!rt.IsContainPoint(leanFinger.ScreenPosition)) return false;

			return true;
		}


		private void OnFingerTap(LeanFinger leanFinger)
		{
			if (IsReadyToInvokeEvents(leanFinger))
				Tap?.Invoke(this, EventArgs.Empty);
		}

		private void OnFingerUp(LeanFinger leanFinger)
		{
			if (leanFinger.Age >= _longTapThreshold && leanFinger.SwipeScreenDelta.magnitude == 0f)
				if (IsReadyToInvokeEvents(leanFinger))
					LongTap?.Invoke(this, EventArgs.Empty);
		}

	}
}