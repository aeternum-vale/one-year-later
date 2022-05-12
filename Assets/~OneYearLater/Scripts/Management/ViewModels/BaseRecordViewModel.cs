using System;

namespace OneYearLater.Management.ViewModels
{
	public abstract class BaseRecordViewModel
	{
		public string Hash { get; set; }
		public ERecordType Type { get; protected set; }
		public DateTime RecordDateTime { get; set; }
		public bool IsImported { get; set; }
		

		protected BaseRecordViewModel(string hash, DateTime dateTime) : this(dateTime)
		{
			Hash = hash;
		}

		protected BaseRecordViewModel(DateTime recordDateTime) : this()
		{
			RecordDateTime = recordDateTime;
		}

		private BaseRecordViewModel() => InitType();
		protected abstract void InitType();
	}
}
