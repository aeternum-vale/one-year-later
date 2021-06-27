using OneYearLater.Management.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
    public interface IViewManager
    {
        void DisplayDate(DateTime date);
        Task DisplayDayFeedAsync(IEnumerable<BaseRecordViewModel> records);
        void DisplayFeedLoadingView();
        void SetIsDatePickingBlocked(bool isBlocked);
        event EventHandler<DateTime> DayChanged;
        event EventHandler<String> XMLFilePicked;

    }
}
