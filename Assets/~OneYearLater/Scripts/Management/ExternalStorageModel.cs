
using System;
namespace OneYearLater.Management
{
	public struct ExternalStorageModel
	{
		public EExternalStorageKey key;
		public string state;
		public DateTime? lastSync;
	}
}