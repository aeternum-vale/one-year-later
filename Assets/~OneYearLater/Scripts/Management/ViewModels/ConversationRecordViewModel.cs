
using System;
namespace OneYearLater.Management.ViewModels
{
	public struct MessageViewModel
	{
		public bool UserIsMessageAuthor { get; set; }
		public DateTime MessageTime { get; set; }
		public string MessageText { get; set; }
	}

	public class ConversationRecordViewModel : BaseRecordViewModel
	{
		public MessageViewModel[] Messages { get; set; }

		public ConversationRecordViewModel(DateTime dateTime, MessageViewModel[] messages) : base(dateTime)
		{
			Messages = messages;
		}
	}
}