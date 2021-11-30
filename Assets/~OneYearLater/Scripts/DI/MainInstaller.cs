using OneYearLater.ExternalStorages;
using OneYearLater.LocalStorages;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.UI;
using OneYearLater.UI.Interfaces;
using OneYearLater.UI.Popups;
using OneYearLater.UI.Views.ScreenViews;
using UnityEngine;
using Zenject;

namespace OneYearLater.DI
{
	public class MainInstaller : MonoInstaller
	{
		[SerializeField] private ViewManager _viewManager;
		[SerializeField] private LeanTouchFacade _leanTouchFacade;
		[SerializeField] private PopupManager _popupManager;

		[Header("Screen Views")]
		[SerializeField] private FeedScreenView _feedScreenView;
		[SerializeField] private ImportScreenView _importScreenView;


		public override void InstallBindings()
		{
			Container.Bind<IViewManager>().To<ViewManager>().FromInstance(_viewManager).AsSingle();
			Container.Bind<ILocalRecordStorage>().To<SQLiteLocalRecordStorage>().FromNew().AsSingle();
			Container.Bind<IAppLocalStorage>().To<SQLiteAppLocalStorage>().FromNew().AsSingle();
			Container.Bind<IMobileInputHandler>().To<LeanTouchFacade>().FromInstance(_leanTouchFacade).AsSingle();
			Container.Bind<IExternalStorage>().FromMethodMultiple(GetExternalStorages).AsSingle();
			Container.Bind<IPopupManager>().To<PopupManager>().FromInstance(_popupManager).AsSingle();
			Container.Bind<Importer>().FromNew().AsSingle();

			Container.Bind<FeedScreenView>().FromInstance(_feedScreenView).AsSingle();
			Container.Bind<IFeedScreen>().To<FeedScreenView>().FromResolve();

			Container.Bind<ImportScreenView>().FromInstance(_importScreenView).AsSingle();
			Container.Bind<IImportScreen>().To<ImportScreenView>().FromResolve();
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
