using SQLite;
using System;

namespace OneYearLater.LocalStorageSQLite.Models
{
	[Table("records")]
	class RecordModel
	{
		[PrimaryKey, AutoIncrement]
		[Column("id")]
		public int Id { get; set; }

		[Column("type")]
		public int Type { get; set; }

		[Column("record_date_time")]
		public DateTime RecordDateTime { get; set; }

		[Column("content")]
		public string Content { get; set; }

		[Column("hash")]
		public string Hash { get; set; }

		[Column("is_deleted")]
		public bool IsDeleted { get; set; }

		[Column("created")]
		public DateTime Created { get; set; }

		[Column("additional_info")]
		public string AdditionalInfo { get; set; }
	}
}
