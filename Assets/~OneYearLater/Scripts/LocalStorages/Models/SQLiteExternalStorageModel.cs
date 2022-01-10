using SQLite;
using System;

namespace OneYearLater.LocalStorages.Models
{
	public class SQLiteExternalStorageModel
	{
		[PrimaryKey]
		public int Id { get; set; }
		public string State { get; set; }
		public DateTime? LastSync { get; set; } = null;
	}
}
