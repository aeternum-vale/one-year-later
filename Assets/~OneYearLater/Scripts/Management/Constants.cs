using System.Collections.Generic;

namespace OneYearLater.Management
{
	public enum ERecordKey
	{
		None = 0,
		Diary
	}

	public enum EExternalStorageKey
	{
        None = 0,
        DropBox,
        PCloud
	}

	public enum EScreenViewKey
	{
		None = 0,
		Feed,
		Settings,
		ExternalStorages
	}

	public enum EExternalStorageViewAppearanceState 
	{
		None = 0,
		NotConnected = 1,
		Connecting = 2,
		Connected = 3,
		Synchronizing = 4
	}

	public static class Constants
	{
		public static readonly Dictionary<ERecordKey, string> RecordTypeNames = new Dictionary<ERecordKey, string>()
		{
			[ERecordKey.Diary] = "diary"
		};
	}
}
