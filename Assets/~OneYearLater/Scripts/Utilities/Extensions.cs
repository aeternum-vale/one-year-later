using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Utilities
{
	public static class Extensions
	{
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
	}
}
