using OneYearLater.Management.Interfaces;

namespace OneYearLater.Management.Controllers
{
	public class SettingsScreenController
	{
		private ISettingsScreenView _view;

		public SettingsScreenController(ISettingsScreenView view)
		{
			_view = view;
		}

	}
}