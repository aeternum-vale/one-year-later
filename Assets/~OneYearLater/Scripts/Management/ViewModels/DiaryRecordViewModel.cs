using System;

namespace OneYearLater.Management.ViewModels
{
	public class DiaryRecordViewModel : BaseRecordViewModel
	{
		public string Text { get; set; }

		public DiaryRecordViewModel(int id, DateTime dateTime, string text) : base(id, dateTime)
		{
			Init(text);
		}

		public DiaryRecordViewModel(DateTime dateTime, string text) : base(dateTime)
		{
			Init(text);
		}

		private void Init(string text)
		{
			Type = ERecordKey.Diary;
			Text = text;
		}
	}
}
