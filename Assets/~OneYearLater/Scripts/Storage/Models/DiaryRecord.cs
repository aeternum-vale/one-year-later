using SQLite;
using System;

namespace OneYearLater.Storage.Models
{
    [Table("diary_records")]
    class DiaryRecordModel
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(10000)]
        public string Text { get; set; } = "";

        //[NotNull]
        public DateTime RecordDateTime { get; set; } = new DateTime();
    }
}
