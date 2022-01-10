using System;

namespace OneYearLater.Management.ViewModels
{
	public abstract class BaseRecordViewModel
	{
		public int Id { get; set; }
		public ERecordKey Type { get; protected set; }
		public DateTime DateTime { get; set; }
		public bool IsImported { get; set; }

		protected BaseRecordViewModel(int id, DateTime dateTime)
		{
			Id = id;
			DateTime = dateTime;
		}

		protected BaseRecordViewModel(DateTime dateTime)
		{
			DateTime = dateTime;
		}
	}
}
