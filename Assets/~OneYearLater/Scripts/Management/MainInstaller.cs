using OneYearLater.Management.Interfaces;
using OneYearLater.LocalStorageSQLite;
using OneYearLater.UI;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
	[SerializeField] ViewManager _viewManager;
	[SerializeField] SQLiteLocalStorage _SQLiteStorage;

	public override void InstallBindings()
	{
		Container.Bind<IViewManager>().FromInstance(_viewManager);
		Container.Bind<ILocalStorage>().FromInstance(_SQLiteStorage);
	}
}
