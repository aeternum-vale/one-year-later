using System;

namespace OneYearLater.Management.Interfaces
{
	public interface IRecordEditorScreenView
	{
		DateTime DateTime { get; set; }
		string Text { get; set; }

		event EventHandler ApplyIntent;
		event EventHandler CancelIntent;
		event EventHandler DeleteIntent;
	}
}
