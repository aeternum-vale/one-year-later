using OneYearLater.ExternalStorages;
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
		[SerializeField] LeanTouchFacade _leanTouchFacade;

		public override void InstallBindings()
		{
			Container
				.Bind<IViewManager>()
				.To<ViewManager>()
				.FromInstance(_viewManager)
				.AsSingle();

			Container
				.Bind<ILocalRecordStorage>()
				.To<SQLiteLocalRecordStorage>()
				.FromNew()
				.AsSingle();

			Container
				.Bind<IAppLocalStorage>()
				.To<SQLiteAppLocalStorage>()
				.FromNew()
				.AsSingle();

			Container
				.Bind<IMobileInputHandler>()
				.To<LeanTouchFacade>()
				.FromInstance(_leanTouchFacade);
				
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
