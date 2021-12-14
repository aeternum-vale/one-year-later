using OneYearLater.Management.ViewModels;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IRecordStorageSynchronizer
	{
		UniTask<bool> SyncLocalAndExternalRecordStoragesAsync(IExternalStorage externalStorage);
	}
}
