using System.IO;
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
		ExternalStorages,
		Import,
		RecordEditor
	}

	public enum EExternalStorageAppearance
	{
		None = 0,
		NotConnected = 1,
		Connecting = 2,
		Connected = 3,
		Synchronizing = 4
	}


	public static class Constants
	{
		public static Dictionary<EExternalStorageAppearance, string> ExternalStorageAppearanceStatuses = new Dictionary<EExternalStorageAppearance, string>()
		{
			[EExternalStorageAppearance.None] = "",
			[EExternalStorageAppearance.NotConnected] = "not connected",
			[EExternalStorageAppearance.Connecting] = "connecting...",
			[EExternalStorageAppearance.Connected] = "connected",
			[EExternalStorageAppearance.Synchronizing] = "synchronization..."
		};

		public static readonly Dictionary<ERecordKey, string> RecordTypeNames = new Dictionary<ERecordKey, string>()
		{
			[ERecordKey.Diary] = "diary"
		};
	}
}
