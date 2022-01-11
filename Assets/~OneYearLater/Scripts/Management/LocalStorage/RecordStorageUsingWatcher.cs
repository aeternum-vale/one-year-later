using System;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.Interfaces.Importers;
using UniRx;

namespace OneYearLater.Management.LocalStorage
{
	public class StorageUsingStatusChangeArgs : EventArgs
	{
		public EStorageUser User { get; set; }
		public bool IsRecordStorageInUse { get; set; }

	}

	public class RecordStorageUsingWatcher
	{
		public event EventHandler<StorageUsingStatusChangeArgs> StorageUsingStatusChange;

		private IDiaryImporter _diaryImporter;
		private IRecordStorageSynchronizer _synchronizer;


		public RecordStorageUsingWatcher(
			IDiaryImporter diaryImporter,
			IRecordStorageSynchronizer synchronizer
		)
		{
			_diaryImporter = diaryImporter;
			_synchronizer = synchronizer;

			_diaryImporter.IsImportingInProcess.Subscribe(isInUse => OnStorageUsingStatusChange(EStorageUser.Importer, isInUse));
			_synchronizer.IsSyncInProcess.Subscribe(isInUse => OnStorageUsingStatusChange(EStorageUser.Synchronizer, isInUse));
		}


		private void OnStorageUsingStatusChange(EStorageUser user, bool isInUse)
		{
			StorageUsingStatusChange?.Invoke(this, 
				new StorageUsingStatusChangeArgs() { User = user, IsRecordStorageInUse = isInUse });
		}

	}
}