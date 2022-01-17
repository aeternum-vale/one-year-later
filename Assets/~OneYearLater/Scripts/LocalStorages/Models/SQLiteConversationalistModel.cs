using SQLite;
using System;


namespace OneYearLater.LocalStorages.Models
{
	public class SQLiteConversationalistModel
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string Name { get; set; }
	}
}