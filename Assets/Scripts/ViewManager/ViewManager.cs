using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using UnityEngine;

namespace OneYearLater.View
{
	public class ViewManager : MonoBehaviour, IViewManager
    {
        public event EventHandler<DateTime> DayChanged;
        public event EventHandler<string> XMLFilePicked;

		public void DisplayDate(DateTime date)
		{
			throw new NotImplementedException();
		}

		public Task DisplayDayFeedAsync(IEnumerable<BaseRecordViewModel> records)
		{
			throw new NotImplementedException();
		}

		public void DisplayFeedLoadingView()
		{
			throw new NotImplementedException();
		}

		public void SetIsDatePickingBlocked(bool isBlocked)
		{
			throw new NotImplementedException();
		}
	}
}
