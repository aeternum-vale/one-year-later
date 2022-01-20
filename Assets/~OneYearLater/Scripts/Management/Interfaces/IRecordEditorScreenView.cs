using System;
using OneYearLater.Management.ViewModels;

namespace OneYearLater.Management.Interfaces
{
	public interface IRecordEditorScreenView
	{
		BaseRecordViewModel EditingRecordViewModel { get; set; }

		event EventHandler ApplyIntent;
		event EventHandler CancelIntent;
		event EventHandler DeleteIntent;
	}
}
