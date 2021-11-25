using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
	public static class Extensions
	{
		public static bool IsAny<T>(this IEnumerable<T> data)
		{
			return data != null && data.Any();
		}

		public static UniTask ToUniTask(this Tween tween)
		{
			return tween.AsyncWaitForCompletion().AsUniTask();
		}

		public static UniTask ToUniTask(this Tween tween, CancellationToken token)
		{
			return tween.AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(token);
		}

		public static void ToDictionary<TKey, TValue>(this SerializableKeyValuePair<TKey, TValue>[] array, out Dictionary<TKey, TValue> dictionary)
		{
			dictionary = new Dictionary<TKey, TValue>();
			foreach (var item in array)
				dictionary[item.Key] = item.Value;
		}

		public static int ActiveChildCount(this Transform t)
		{
			int k = 0;

			foreach (Transform c in t)
				if (c.gameObject.activeSelf) k++;

			return k;
		}

	}
}
