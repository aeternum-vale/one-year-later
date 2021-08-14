using System.Collections.Generic;
using OneYearLater.Management.Interfaces;

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

	public static class Constants
	{
		public static readonly Dictionary<ERecordKey, string> RecordTypeNames = new Dictionary<ERecordKey, string>()
		{
			[ERecordKey.Diary] = "diary"
		};
	}
}
