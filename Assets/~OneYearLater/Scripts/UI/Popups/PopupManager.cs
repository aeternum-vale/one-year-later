using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.UI.Interfaces;
using UnityEngine;
using Utilities;

namespace OneYearLater.UI.Popups
{
	public class PopupManager : MonoBehaviour
	{
		[SerializeField] private PopupSPair[] _popupPrefabsArray;
		private Dictionary<EPopupKey, Popup> _popupPrefabsDictionary;
		private Dictionary<EPopupKey, Popup> _popupCache = new Dictionary<EPopupKey, Popup>();

		[SerializeField] private CanvasGroupFader _background;
		[SerializeField] private Transform _container;


		#region Unity Callbacks
		private void Awake()
		{
			_popupPrefabsArray.ToDictionary(out _popupPrefabsDictionary);
		}

		#endregion

		private async UniTask ShowPopupAsync(Popup popup)
		{
			_background.UnfadeAsync().Forget();
			popup.UnfadeAsync().Forget();

			bool isOkClicked = false;
			EventHandler clickHandler = (s, a) => isOkClicked = true;
			popup.OkButtonClicked += clickHandler;
			await UniTask.WaitUntil(() => isOkClicked);
			popup.OkButtonClicked -= clickHandler;

			await popup.FadeAsync();
			await _background.FadeAsync();
		}

		public async UniTask ShowMessagePopupAsync(string messageText)
		{
			Popup messagePopup;
			EPopupKey key = EPopupKey.Message;

			if (_popupCache.TryGetValue(key, out messagePopup))
				_popupCache[key] = null;
			else
				messagePopup = Instantiate(_popupPrefabsDictionary[EPopupKey.Message], _container);

			messagePopup.Init(messageText, string.Empty);
			await ShowPopupAsync(messagePopup);
			_popupCache[EPopupKey.Message] = messagePopup;
		}


	}
}
