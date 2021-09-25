using SQLite;
using System;

namespace OneYearLater.LocalStorages.Models
{
	[Table("external_storage")]
	class SQLiteExternalStorageModel
	{
		[PrimaryKey]
		[Column("id")]
		public int Id { get; set; }

		[Column("state")]
		public string State { get; set; }

		[Column("date_time")]
		public DateTime? LastSync { get; set; } = null;
	}
}
