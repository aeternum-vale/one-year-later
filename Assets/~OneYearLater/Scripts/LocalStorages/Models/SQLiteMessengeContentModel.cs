using SQLite;
using System;

namespace OneYearLater.LocalStorages.Models
{
	public class SQLiteMessengeContentModel
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string MessageText { get; set; }
		public bool IsFromUser { get; set; }
		public int ConversationalistId { get; set; }
	}
}