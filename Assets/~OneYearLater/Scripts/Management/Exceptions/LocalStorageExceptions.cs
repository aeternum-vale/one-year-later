
using System;
namespace OneYearLater.Management.Exceptions
{
	public class LocalStorageException : Exception
	{
		public LocalStorageException() { }
		public LocalStorageException(string message) : base(message) { }
		public LocalStorageException(string message, Exception innerException) : base(message, innerException) { }
	}

	public class RecordDuplicateException : LocalStorageException
	{
		public RecordDuplicateException() { }
		public RecordDuplicateException(string message) : base(message) { }
		public RecordDuplicateException(string message, Exception innerException) : base(message, innerException) { }

	}


}