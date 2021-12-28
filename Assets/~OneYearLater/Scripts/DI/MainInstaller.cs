using OneYearLater.ExternalStorages;
using OneYearLater.Import;
using OneYearLater.LocalStorages;
using OneYearLater.Management.Controllers;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.LocalStorage;
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

		[SerializeField] private SideMenuView _sideMenu;

		[Header("Screen Views")]
		[SerializeField] private FeedScreenView _feedScreenView;
		[SerializeField] private ImportScreenView _importScreenView;
		[SerializeField] private ExternalStoragesScreenView _externalStoragesScreenView;
		[SerializeField] private RecordEditorScreenView _recordEditorScreenView;


		public override void InstallBindings()
		{

			Container.Bind<SideMenuView>().FromInstance(_sideMenu).AsSingle();
			Container.Bind<IScreensMenuView>().To<SideMenuView>().FromResolve();
			
			Container.Bind<IViewManager>().To<ViewManager>().FromInstance(_viewManager).AsSingle();
			Container.Bind<IMobileInputHandler>().To<LeanTouchFacade>().FromInstance(_leanTouchFacade).AsSingle();
			
			Container.Bind<RecordStorageConnector>().FromNew().AsSingle();
			Container.Bind<IRecordStorageConnector>().To<RecordStorageConnector>().FromResolve();

			Container.Bind<SQLiteLocalRecordStorage>().FromNew().AsSingle();
			Container.Bind<ILocalRecordStorage>().To<HandledSQLiteLocalRecordStorage>().FromNew().AsSingle();
			Container.Bind<HandledLocalStorage>().FromNew().AsSingle();
			
			Container.Bind<IAppLocalStorage>().To<SQLiteAppLocalStorage>().FromNew().AsSingle();
			Container.Bind<IExternalStorage>().FromMethodMultiple(GetExternalStorages).AsSingle();

			Container.Bind<IRecordStorageSynchronizer>().To<SQLiteSynchronizer>().FromNew().AsSingle();

			Container.Bind<PopupManager>().FromInstance(_popupManager).AsSingle();
			Container.Bind<IPopupManager>().To<PopupManager>().FromResolve();

			Container.Bind<IImporter>().To<Importer>().FromNew().AsSingle();

			Container.Bind<FeedScreenView>().FromInstance(_feedScreenView).AsSingle();
			Container.Bind<IFeedScreenView>().To<FeedScreenView>().FromResolve();
			Container.Bind<FeedScreenController>().FromNew().AsSingle();

			Container.Bind<ImportScreenView>().FromInstance(_importScreenView).AsSingle();
			Container.Bind<IImportScreenView>().To<ImportScreenView>().FromResolve();
			Container.Bind<ImportScreenController>().FromNew().AsSingle();

			Container.Bind<ExternalStoragesScreenView>().FromInstance(_externalStoragesScreenView).AsSingle();
			Container.Bind<IExternalStoragesScreenView>().To<ExternalStoragesScreenView>().FromResolve();
			Container.Bind<ExternalStoragesScreenController>().FromNew().AsSingle();

			Container.Bind<RecordEditorScreenView>().FromInstance(_recordEditorScreenView).AsSingle();
			Container.Bind<IRecordEditorScreenView>().To<RecordEditorScreenView>().FromResolve();
			Container.Bind<RecordEditorScreenController>().FromNew().AsSingle();

			Container.Bind<IScreensMediator>().To<ScreensMediator>().FromNew().AsSingle();
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
