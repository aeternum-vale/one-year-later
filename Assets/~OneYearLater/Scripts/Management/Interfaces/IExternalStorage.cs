using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IExternalStorage
	{
		UniTask Authenticate();
		UniTask Sync();

	}
}
