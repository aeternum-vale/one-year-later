using System;
using System.Collections.Generic;

namespace OneYearLater.Management
{
	public enum ERecordKey
	{
		None = 0,
		Diary,
		Conversation
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

	public enum EStorageUser
	{
		None = 0,
		Feed,
		Importer,
		Synchronizer
	}

	public enum EExternalStorageAppearance
	{
		None = 0,
		NotConnected,
		Connecting,
		Connected,
		Synchronizing,
		Error,
		Waiting
	}


	public static class Constants
	{
		public const string HandledRecordStorageId = "HandledRecordStorageId";
		public static Dictionary<EExternalStorageAppearance, string> ExternalStorageAppearanceDefaultStatuses = new Dictionary<EExternalStorageAppearance, string>()
		{
			[EExternalStorageAppearance.None] = "",
			[EExternalStorageAppearance.NotConnected] = "not connected",
			[EExternalStorageAppearance.Connecting] = "connecting...",
			[EExternalStorageAppearance.Connected] = "connected",
			[EExternalStorageAppearance.Synchronizing] = "synchronization...",
			[EExternalStorageAppearance.Error] = "an error was occurred",
			[EExternalStorageAppearance.Waiting] = "wait...",
		};

		public static readonly Dictionary<ERecordKey, string> RecordTypeNames = new Dictionary<ERecordKey, string>()
		{
			[ERecordKey.Diary] = "diary"
		};
	}

	public struct ImportResult
	{
		public bool IsCanceled;
		public int ImportedRecordsCount;
		public int AbortedDuplicatesCount;
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
