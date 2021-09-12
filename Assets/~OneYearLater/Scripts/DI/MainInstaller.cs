using ExternalStorages;
using OneYearLater.LocalStorages;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI;
using OneYearLater.UI.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.DI
{
	public class MainInstaller : MonoInstaller
	{
		[SerializeField] ViewManager _viewManager;
		[SerializeField] SQLiteLocalStorage _SQLiteStorage;
		[SerializeField] LeanTouchFacade _leanTouchFacade;

		public override void InstallBindings()
		{
			Container
				.Bind<IViewManager>()
				.To<ViewManager>()
				.FromInstance(_viewManager)
				.AsSingle();

			Container
				.Bind<ILocalStorage>()
				.To<SQLiteLocalStorage>()
				.FromInstance(_SQLiteStorage)
				.AsSingle();

			Container
				.Bind<IMobileInputHandler>()
				.To<LeanTouchFacade>()
				.FromInstance(_leanTouchFacade)
				.AsSingle();

			Container
				.Bind<IExternalStorage>()
				.FromMethodMultiple(GetExternalStorages)
				.AsSingle();
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
}
