using System;
using OneYearLater.Management.Interfaces;
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

		private IImporter _importer;
		private IRecordStorageSynchronizer _synchronizer;


		public RecordStorageUsingWatcher(
			IImporter importer,
			IRecordStorageSynchronizer synchronizer
		)
		{
			_importer = importer;
			_synchronizer = synchronizer;

			_importer.IsImportingInProcess.Subscribe(isInUse => OnStorageUsingStatusChange(EStorageUser.Importer, isInUse));
			_synchronizer.IsSyncInProcess.Subscribe(isInUse => OnStorageUsingStatusChange(EStorageUser.Synchronizer, isInUse));
		}


		private void OnStorageUsingStatusChange(EStorageUser user, bool isInUse)
		{
			StorageUsingStatusChange?.Invoke(this, 
				new StorageUsingStatusChangeArgs() { User = user, IsRecordStorageInUse = isInUse });
		}

	}
}