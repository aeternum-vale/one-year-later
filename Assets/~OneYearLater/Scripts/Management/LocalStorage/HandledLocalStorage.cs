using System;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management.LocalStorage
{
	public class HandledLocalStorage : AbstractHandledLocalStorage
	{
		[Inject] private ILocalRecordStorage _localRecordStorage;
		[Inject] private IPopupManager _popupManager;


		protected override ILocalRecordStorage LocalRecordStorage => _localRecordStorage;

		protected override async UniTask Handle(UniTask operation)
		{
			try
			{
				await operation;
			}
			catch (RecordDuplicateException ex)
			{
				throw ex;
			}
			catch (LocalStorageException ex)
			{
				Handle(ex);
				throw ex;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				throw ex;
			}
		}

		protected override async UniTask<T> Handle<T>(UniTask<T> operation)
		{
			try
			{
				T result = await operation;
				return result;
			}
			catch (RecordDuplicateException ex)
			{
				throw ex;
			}
			catch (LocalStorageException ex)
			{
				Handle(ex);
				throw ex;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				throw ex;
			}
		}

		private void Handle(Exception ex)
		{
			Debug.LogError(ex.Message);
			_popupManager.RunMessagePopupAsync("An error was occurred");
		}
	}
}