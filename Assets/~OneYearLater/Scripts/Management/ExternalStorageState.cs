
using System;
namespace OneYearLater.Management
{
	public struct ExternalStorageModel
	{
		public EExternalStorageKey key;
		public string serializedData;
		public DateTime lastSync;
	}
}