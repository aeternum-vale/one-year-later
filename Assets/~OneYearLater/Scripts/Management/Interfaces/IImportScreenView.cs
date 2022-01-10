using System;
namespace OneYearLater.Management.Interfaces
{
	public interface IImportScreenView
	{
		bool IsImportFromTextFileButtonInteractable { get; set; }
		event EventHandler ImportFromTextFileIntent;
		bool IsImportFromTextFileInProgress { get; set; }
		void SetImportFromTextFileProgress(float value);
	}
}