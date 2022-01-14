using System;

namespace OneYearLater.Management.ViewModels
{

	public class MessageContent
	{
		public bool UserIsMessageAuthor { get; set; }
		public string MessageText { get; set; }
		public string CompanionName { get; set; }

	}

	public class MessageRecordViewModel : BaseRecordViewModel
	{
		public MessageContent Content { get; set; } = new MessageContent();

		public MessageRecordViewModel(int id, DateTime dateTime) : base(id, dateTime)
		{
			Init();
		}

		public MessageRecordViewModel(DateTime dateTime) : base(dateTime)
		{
			Init();
		}

		private void Init()
		{
			Type = ERecordKey.Message;
		}
	}
}