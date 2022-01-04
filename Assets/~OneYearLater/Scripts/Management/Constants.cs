using System;
using System.Collections.Generic;

namespace OneYearLater.Management
{
	public enum ERecordKey
	{
		None = 0,
		Diary
	}

	public enum EExternalStorageKey
	{
		None = 0,
		DropBox,
		PCloud
	}

	public enum EScreenViewKey
	{
		None = 0,
		Feed,
		Settings,
		ExternalStorages,
		Import,
		RecordEditor
	}

	public enum EExternalStorageAppearance
	{
		None = 0,
		NotConnected = 1,
		Connecting = 2,
		Connected = 3,
		Synchronizing = 4,
		Error = 5
	}


	public static class Constants
	{
		public static Dictionary<EExternalStorageAppearance, string> ExternalStorageAppearanceDefaultStatuses = new Dictionary<EExternalStorageAppearance, string>()
		{
			[EExternalStorageAppearance.None] = "",
			[EExternalStorageAppearance.NotConnected] = "not connected",
			[EExternalStorageAppearance.Connecting] = "connecting...",
			[EExternalStorageAppearance.Connected] = "connected",
			[EExternalStorageAppearance.Synchronizing] = "synchronization...",
			[EExternalStorageAppearance.Error] = "an error was occurred"
		};

		public static readonly Dictionary<ERecordKey, string> RecordTypeNames = new Dictionary<ERecordKey, string>()
		{
			[ERecordKey.Diary] = "diary"
		};
	}

	public interface IAsyncResult
	{
		Exception Error { get; set; }
	}

	public struct AsyncResult : IAsyncResult
	{
		public Exception Error { get; set; }
	}


	public struct AsyncResult<T> : IAsyncResult where T : class
	{
		public T Data { get; set; }
		public Exception Error { get; set; }
	}

	public struct SimpleAsyncResult<T> : IAsyncResult where T : struct
	{
		public T? Data { get; set; }
		public Exception Error { get; set; }
	}

	public static class AsyncResultExtensions
	{
		public static bool IsSuccessful(this IAsyncResult ar)
		{
			return ar.Error == null;
		}

		public static bool IsError(this IAsyncResult ar)
		{
			return ar.Error != null;
		}
	}
}
