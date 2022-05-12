using SQLite;
using System;


namespace OneYearLater.LocalStorages.Models
{
	public class SQLiteConversationalistModel
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		
		[Unique]
		public string Hash { get; set; }
		public string Name { get; set; }
		
		public DateTime Created { get; set; }
		public DateTime LastEdited { get; set; }
	}
}