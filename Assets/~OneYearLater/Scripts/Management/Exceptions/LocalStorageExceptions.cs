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

	}

	public class CannotAccessLocalStorageException : LocalStorageException
	{
		public CannotAccessLocalStorageException() { }
		public CannotAccessLocalStorageException(string message) : base(message) { }
		public CannotAccessLocalStorageException(string message, Exception innerException) : base(message, innerException) { }
	}

	public class RecordStorageOccupiedException : LocalStorageException
	{
		public RecordStorageOccupiedException() { }
		public RecordStorageOccupiedException(string message) : base(message) { }

	}
}
