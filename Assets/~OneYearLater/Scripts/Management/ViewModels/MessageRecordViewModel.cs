using System;

namespace OneYearLater.Management.ViewModels
{
	public class MessageRecordViewModel : BaseRecordViewModel
	{
		public bool IsFromUser { get; set; }
		public string MessageText { get; set; }
		public string ConversationalistName { get; set; }

		public MessageRecordViewModel(int id, DateTime dateTime) : base(id, dateTime) { }

		public MessageRecordViewModel(DateTime dateTime) : base(dateTime) { }

		protected override void InitType() { Type = ERecordType.Message; }
	}
}