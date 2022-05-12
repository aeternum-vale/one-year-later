using SQLite;
using System;

namespace OneYearLater.LocalStorages.Models
{
	public class SQLiteMessageContentModel
	{
		[PrimaryKey]
		public string Hash { get; set; }
		public string MessageText { get; set; }
		public bool IsFromUser { get; set; }
		public int ConversationalistId { get; set; }
	}
}