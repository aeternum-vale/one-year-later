using OneYearLater.Management.Interfaces;
using OneYearLater.Storage;
using OneYearLater.View;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
	[SerializeField] ViewManager _viewManager;
	[SerializeField] SQLiteStorage _SQLiteStorage;

	public override void InstallBindings()
	{
		Container.Bind<IViewManager>().FromInstance(_viewManager);
		Container.Bind<IStorage>().FromInstance(_SQLiteStorage);
	}
}
