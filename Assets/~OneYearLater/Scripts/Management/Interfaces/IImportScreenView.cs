using System;
namespace OneYearLater.Management.Interfaces
{
	public interface IImportScreenView
	{
		event EventHandler ImportFromTextFileIntent;
		void SetIsImportFromTextFileInProgress(bool isInProgress);
		void SetImportFromTextFileProgressValue(float progress);
	}
}