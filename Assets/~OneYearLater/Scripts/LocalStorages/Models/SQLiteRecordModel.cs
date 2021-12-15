using SQLite;
using System;

namespace OneYearLater.LocalStorages.Models
{
	class SQLiteRecordModel
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public int Type { get; set; }
		public DateTime RecordDateTime { get; set; }
		public string Content { get; set; }
		public string Hash { get; set; }
		public bool IsLocal { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastEdited { get; set; }
		public string AdditionalInfo { get; set; }
	}
}
