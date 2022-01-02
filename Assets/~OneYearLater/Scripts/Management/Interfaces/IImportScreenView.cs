using System;
namespace OneYearLater.Management.Interfaces
{
	public interface IImportScreenView
	{
		event EventHandler ImportFromTextFileIntent;
		bool IsImportFromTextFileInProgress { get; set; }
		void SetImportFromTextFileProgress(float value);
	}
}