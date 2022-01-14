using System;
namespace OneYearLater.Management.Interfaces
{
	public interface IImportScreenView
	{
		bool IsImportingAllowed {get; set;}

		event EventHandler<EImportType> ImportIntent;
		bool IsImportInProgress(EImportType type);
		void SetIsImportInProgress(EImportType type, bool isImportInProgress);
		void SetImportFileProgress(EImportType type, float value);

	}
}