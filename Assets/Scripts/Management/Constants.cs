using System.Collections.Generic;

namespace OneYearLater.Management
{
    public enum ERecord
    {
        None = 0,
        Diary
    }

    public static class Constants
    {
        public static readonly Dictionary<ERecord, string> RecordTypeToString = new Dictionary<ERecord, string>()
        {
            [ERecord.Diary] = "diary"
        };
    }
}
