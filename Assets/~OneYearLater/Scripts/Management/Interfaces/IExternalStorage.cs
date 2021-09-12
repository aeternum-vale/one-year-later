﻿using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IExternalStorage
	{
		EExternalStorageKey Key { get; }
		string Name { get; }

		void RequestAccessCode();
		UniTask<bool> Connect(string code);
		UniTask Synchronize();
		UniTask<bool> IsFileExist(string path);
		UniTask DownloadFile(string externalStoragePath, string localStoragePath);
		UniTask UploadFile(string localStoragePath, string externalStoragePath);
	}
}
