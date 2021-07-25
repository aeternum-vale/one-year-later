using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IViewManager
	{
		UniTask DisplayDayFeedAsync(DateTime date, IEnumerable<BaseRecordViewModel> records);
		void SetIsDatePickingBlocked(bool isBlocked);
		void DisplayFeedLoading();
		event EventHandler<DateTime> DayChanged;
		event EventHandler<String> XMLFilePicked;
	}
}
