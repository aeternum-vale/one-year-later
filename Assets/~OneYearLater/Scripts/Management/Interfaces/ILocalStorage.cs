﻿using OneYearLater.Management.ViewModels;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface ILocalStorage
	{
		UniTask InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records);
		UniTask<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date);
		UniTask<bool> SynchronizeLocalAndExternal(IExternalStorage externalStorage);
	}
}
