using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IFeedScreenView
	{
		event EventHandler<DateTime> DayChangeIntent;
		event EventHandler AddRecordIntent;
		event EventHandler<string> EditRecordIntent;

		UniTask DisplayDayFeedAsync(DateTime date, IEnumerable<BaseRecordViewModel> records);
		void SetIsDatePickingBlocked(bool isBlocked);
		void DisplayThatFeedIsLoading();

	}
}