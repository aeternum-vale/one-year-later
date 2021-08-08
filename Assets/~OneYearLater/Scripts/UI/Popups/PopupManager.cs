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
			_background.UnfadeAsync(Constants.PopupBackgroundFadeDuration).Forget();

			await UniTask.WhenAll(
				popup.UnfadeAsync(),
				popup.PlayShowAnimation()
			);

			bool isOkClicked = false;
			EventHandler clickHandler = (s, a) => isOkClicked = true;
			popup.OkButtonClicked += clickHandler;
			await UniTask.WaitUntil(() => isOkClicked);
			popup.OkButtonClicked -= clickHandler;

			_background.FadeAsync(Constants.PopupBackgroundFadeDuration).Forget();

			await UniTask.WhenAll(
				popup.FadeAsync(),
				popup.PlayHideAnimation()
			);
		}

		public async UniTask ShowMessagePopupAsync(string messageText, string okButtonText = "OK")
		{
			Popup messagePopup;
			EPopupKey key = EPopupKey.Message;

			if (_popupCache.TryGetValue(key, out messagePopup))
				_popupCache.Remove(key);
			else
				messagePopup = Instantiate(_popupPrefabsDictionary[key], _container);

			messagePopup.Init(messageText, okButtonText);
			await ShowPopupAsync(messagePopup);

			_popupCache[key] = messagePopup;
		}

		public async UniTask<string> ShowPromptPopupAsync(string messageText, string okButtonText = "OK", string placeholderText = "")
		{
			Popup basePopup;
			EPopupKey key = EPopupKey.Promt;

			if (_popupCache.TryGetValue(key, out basePopup))
				_popupCache.Remove(key);
			else
				basePopup = Instantiate(_popupPrefabsDictionary[key], _container);

			PromptPopup promptPopup = basePopup.GetComponent<PromptPopup>();

			promptPopup.Init(messageText, okButtonText, placeholderText);

			await ShowPopupAsync(basePopup);
			_popupCache[key] = basePopup;
			return promptPopup.InputFieldText;
		}


	}
}
