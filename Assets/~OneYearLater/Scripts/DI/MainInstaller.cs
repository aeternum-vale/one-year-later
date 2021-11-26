using OneYearLater.ExternalStorages;
using OneYearLater.LocalStorages;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI;
using OneYearLater.UI.Interfaces;
using OneYearLater.UI.Popups;
using UnityEngine;
using Zenject;

namespace OneYearLater.DI
{
	public class MainInstaller : MonoInstaller
	{
		[SerializeField] private ViewManager _viewManager;
		[SerializeField] private LeanTouchFacade _leanTouchFacade;
		[SerializeField] private PopupManager _popupManager;


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
				.FromInstance(_leanTouchFacade)
				.AsSingle();
				
			Container
				.Bind<IExternalStorage>()
				.FromMethodMultiple(GetExternalStorages)
				.AsSingle();

			Container
				.Bind<IPopupManager>()
				.To<PopupManager>()
				.FromInstance(_popupManager)
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
