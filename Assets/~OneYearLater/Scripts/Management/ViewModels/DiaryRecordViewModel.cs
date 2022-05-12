using System;

namespace OneYearLater.Management.ViewModels
{
	public class DiaryRecordViewModel : BaseRecordViewModel
	{
		public string Text { get; set; }

		public DiaryRecordViewModel(string hash, DateTime dateTime, string text) : base(hash, dateTime) { Text = text; }
		public DiaryRecordViewModel(DateTime recordDateTime) : base(recordDateTime) { }
		public DiaryRecordViewModel(DateTime recordDateTime, string text) : base(recordDateTime) { Text = text; }
		protected override void InitType() { Type = ERecordType.Diary; }
	}
}
