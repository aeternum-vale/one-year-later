using SQLite;
using System;

namespace OneYearLater.LocalStorages.Models
{
	public class SQLiteDiaryContentModel
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string Text { get; set; }
	}
}