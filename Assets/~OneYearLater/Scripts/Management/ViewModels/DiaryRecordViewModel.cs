using System;

namespace OneYearLater.Management.ViewModels
{
	public class DiaryRecordViewModel : BaseRecordViewModel
	{
		public string Text { get; set; }

		public DiaryRecordViewModel(int id, DateTime dateTime, string text) : base(id, dateTime) { Text = text; }
		public DiaryRecordViewModel(DateTime dateTime) : base(dateTime) { }
		public DiaryRecordViewModel(DateTime dateTime, string text) : base(dateTime) { Text = text; }
		protected override void InitType() { Type = ERecordType.Diary; }
	}
}
