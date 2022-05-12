using System;

namespace OneYearLater.Management.ViewModels
{
    public class MessageRecordViewModel : BaseRecordViewModel
    {
        public bool IsFromUser { get; set; }
        public string MessageText { get; set; }

        public ConversationalistViewModel Conversationalist { get; set; }

        public MessageRecordViewModel(string hash, DateTime dateTime) : base(hash, dateTime)
        {
        }

        public MessageRecordViewModel(DateTime recordDateTime) : base(recordDateTime)
        {
        }

        protected override void InitType()
        {
            Type = ERecordType.Message;
        }
    }
}