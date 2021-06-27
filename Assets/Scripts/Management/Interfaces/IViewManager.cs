using System;
using System.Collections.Generic;
using OneYearLater.Management.ViewModels;
using UniRx.Async;

namespace OneYearLater.Management.Interfaces
{
    public interface IViewManager
    {
        void DisplayDate(DateTime date);
		UniTask DisplayDayFeedAsync(IEnumerable<BaseRecordViewModel> records);
		void DisplayFeedLoading();
        void SetIsDatePickingBlocked(bool isBlocked);
        event EventHandler<DateTime> DayChanged;
        event EventHandler<String> XMLFilePicked;

    }
}
