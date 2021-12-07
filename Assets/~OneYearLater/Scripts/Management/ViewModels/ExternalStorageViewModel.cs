using System;

namespace OneYearLater.Management.ViewModels
{
	public struct ExternalStorageViewModel
	{
		public EExternalStorageKey key;
		public string name;
		public string state;
		public DateTime? lastSync;
	}
}
