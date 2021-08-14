using System;

namespace OneYearLater.Management.ViewModels
{
    public abstract class BaseRecordViewModel
    {
        public ERecordKey Type { get; protected set; }

        public DateTime DateTime { get; set; }

        protected BaseRecordViewModel(DateTime dateTime)
        {
            DateTime = dateTime;
        }
    }
}
