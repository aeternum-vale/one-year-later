using System;

namespace OneYearLater.Management.Interfaces
{
	public interface IScreensMenuView
	{
		event EventHandler<EScreenViewKey> ScreenChangeIntent;
		bool IsOpened { get; }
		void Open();
		void Close();
	}
}