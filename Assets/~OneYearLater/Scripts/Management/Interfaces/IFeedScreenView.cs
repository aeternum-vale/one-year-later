using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IFeedScreenView
	{
		event EventHandler<DateTime> DayChanged;
		UniTask DisplayDayFeedAsync(DateTime date, IEnumerable<BaseRecordViewModel> records);
		void SetIsDatePickingBlocked(bool isBlocked);
		void DisplayThatFeedIsLoading();
	}
}