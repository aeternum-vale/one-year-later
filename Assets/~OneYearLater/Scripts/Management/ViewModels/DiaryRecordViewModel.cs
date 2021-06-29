using System;

namespace OneYearLater.Management.ViewModels
{
    public class DiaryRecordViewModel : BaseRecordViewModel
    {
        public string Text { get; set; }

        public DiaryRecordViewModel(DateTime dateTime, string text) : base(dateTime)
        {
            Type = ERecord.Diary;
            Text = text;
        }
    }
}
