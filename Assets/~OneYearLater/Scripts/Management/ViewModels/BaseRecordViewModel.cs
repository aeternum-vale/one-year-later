using System;

namespace OneYearLater.Management.ViewModels
{
	public abstract class BaseRecordViewModel
	{
		public int Id { get; set; }
		public ERecordType Type { get; protected set; }
		public DateTime DateTime { get; set; }
		public bool IsImported { get; set; }

		protected BaseRecordViewModel(int id, DateTime dateTime) : this(dateTime)
		{
			Id = id;
		}

		protected BaseRecordViewModel(DateTime dateTime) : this()
		{
			DateTime = dateTime;
		}

		private BaseRecordViewModel() => InitType();
		protected abstract void InitType();
	}
}
