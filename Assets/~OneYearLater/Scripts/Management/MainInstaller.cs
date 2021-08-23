using ExternalStorages;
using OneYearLater.LocalStorageSQLite;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI;
using OneYearLater.UI.Interfaces;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
	[SerializeField] ViewManager _viewManager;
	[SerializeField] SQLiteLocalStorage _SQLiteStorage;
	[SerializeField] LeanTouchFacade _leanTouchFacade;

	public override void InstallBindings()
	{
		Container.Bind<IViewManager>().FromInstance(_viewManager);
		Container.Bind<ILocalStorage>().FromInstance(_SQLiteStorage);
		Container.Bind<IMobileInputHandler>().FromInstance(_leanTouchFacade);

		Container.Bind<IExternalStorage>().FromMethodMultiple(GetExternalStorages);
	}

	IExternalStorage[] GetExternalStorages(InjectContext context)
	{
		return new IExternalStorage[]
		{
			new DropBoxExternalStorage(),
			new PCloudExternalStorage()
		};
	}
}
