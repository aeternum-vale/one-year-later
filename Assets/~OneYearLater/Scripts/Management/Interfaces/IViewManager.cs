namespace OneYearLater.Management.Interfaces
{
	public interface IViewManager
	{
		void SetScreenView(EScreenViewKey screenViewKey);
		void BlockScreen();
		void UnblockScreen();
	}
}
