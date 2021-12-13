using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OneYearLater.UI
{
	public static class Utils
	{
		public static bool IsContainPoint(this RectTransform rt, Vector2 point)
		{
			return GetRectTransformBounds(rt).Contains(point);
		}

		public static bool IsFullyInsideScreen(this RectTransform rectTransform)
		{
			return CountCornersVisible(rectTransform) == 4; // True if all 4 corners are visible
		}
		public static bool IsPartlyInsideScreen(this RectTransform rectTransform)
		{
			return CountCornersVisible(rectTransform) > 0; // True if any corners are visible
		}

		public static async UniTask RebuildLayout(this RectTransform layoutGroup)
		{
			await UniTask.WaitForEndOfFrame();
			LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
		}

		private static Bounds GetRectTransformBounds(RectTransform transform)
		{
			Vector3[] worldCorners = new Vector3[4];
			transform.GetWorldCorners(worldCorners);
			Bounds bounds = new Bounds(worldCorners[0], Vector3.zero);
			for (int i = 1; i < 4; ++i)
				bounds.Encapsulate(worldCorners[i]);

			return bounds;
		}

		private static int CountCornersVisible(this RectTransform rectTransform)
		{
			var camera = Camera.main;

			Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
			Bounds rtBounds = GetRectTransformBounds(rectTransform);

			var center = rtBounds.center;

			var corner1 = center + new Vector3(rtBounds.extents.x, 0, 0);
			var corner2 = center - new Vector3(rtBounds.extents.x, 0, 0);
			var corner3 = center + new Vector3(0, rtBounds.extents.y, 0);
			var corner4 = center - new Vector3(0, rtBounds.extents.y, 0);

			int cornerCount = 0;
			cornerCount += screenBounds.Contains(corner1) ? 1 : 0;
			cornerCount += screenBounds.Contains(corner2) ? 1 : 0;
			cornerCount += screenBounds.Contains(corner3) ? 1 : 0;
			cornerCount += screenBounds.Contains(corner4) ? 1 : 0;

			return cornerCount;
		}

	}

}