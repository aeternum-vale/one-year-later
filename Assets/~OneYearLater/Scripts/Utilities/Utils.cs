using System;

namespace Utilities
{
	public static class Utils
	{

	}

	[Serializable]
	public class SerializableKeyValuePair<TKey, TValue>
	{
		public TKey Key;
		public TValue Value;
	}
}