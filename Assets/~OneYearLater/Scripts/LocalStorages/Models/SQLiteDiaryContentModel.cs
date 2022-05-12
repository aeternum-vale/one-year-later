using SQLite;
using System;

namespace OneYearLater.LocalStorages.Models
{
	public class SQLiteDiaryContentModel
	{
		[PrimaryKey]
		public string Hash { get; set; }
		public string Text { get; set; }
	}
}