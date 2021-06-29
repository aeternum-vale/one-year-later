using SQLite;
using System;

namespace OneYearLater.Storage.Models
{
    [Table("Records")]
    class Record
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime DateTime;
    }
}
