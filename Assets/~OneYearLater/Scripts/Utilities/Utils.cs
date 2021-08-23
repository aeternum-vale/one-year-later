using System;
using System.Security.Cryptography;
using System.Text;

namespace Utilities
{
	public static class Utils
	{
		private static byte[] GetHash(string inputString)
		{
			using (HashAlgorithm algorithm = SHA256.Create())
				return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		public static string GetSHA256Hash(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
				sb.Append(b.ToString("X2"));

			return sb.ToString().ToLower();
		}
	}

	[Serializable]
	public class SerializableKeyValuePair<TKey, TValue>
	{
		public TKey Key;
		public TValue Value;
	}
	
}