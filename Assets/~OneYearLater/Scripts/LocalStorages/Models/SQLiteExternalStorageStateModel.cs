using SQLite;
using System;

namespace OneYearLater.LocalStorages.Models
{
	[Table("external_storage_state")]
	class SQLiteExternalStorageStateModel
	{
		[PrimaryKey]
		[Column("id")]
		public int Id { get; set; }

		[Column("data")]
		public string State { get; set; }
	}
}
