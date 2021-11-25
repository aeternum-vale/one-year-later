using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.UI.Interfaces;
using UnityEngine;
using Utilities;

namespace OneYearLater.UI.Popups
{
	public class PopupManager : MonoBehaviour //TODO: remove code duplicating
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

			if (_container.ActiveChildCount() == 1)
				_background.FadeAsync(Constants.PopupBackgroundFadeDuration).Forget();

			await UniTask.WhenAll(
				popup.FadeAsync(),
				popup.PlayHideAnimation()
			);
		}


		public async UniTask ShowMessagePopupAsync(string messageText, string okButtonText = "OK")
		{
			EPopupKey key = EPopupKey.Message;
			Popup messagePopup = InstantiatePopup(key);
			messagePopup.Init(messageText, okButtonText);

			await ShowPopupAsync(messagePopup);
			_popupCache[key] = messagePopup;
		}

		public async UniTask<string> ShowPromptPopupAsync(string messageText, string okButtonText = "OK", string placeholderText = "")
		{
			var key = EPopupKey.Promt;
			Popup abstractPopup = InstantiatePopup(EPopupKey.Promt);
			PromptPopup promptPopup = abstractPopup.GetComponent<PromptPopup>();

			promptPopup.Init(messageText, okButtonText, placeholderText);

			await ShowPopupAsync(abstractPopup);
			_popupCache[key] = abstractPopup;
			return promptPopup.InputFieldText;
		}

		private ConfirmPopup InstantiateConfirmPopup(string messageText)
		{
			Popup abstractPopup = InstantiatePopup(EPopupKey.Confirm);
			ConfirmPopup confirmPopup = abstractPopup.GetComponent<ConfirmPopup>();

			confirmPopup.Init(messageText);
			return confirmPopup;
		}

		private Popup InstantiatePopup(EPopupKey key)
		{
			Popup abstractPopup;

			if (_popupCache.TryGetValue(key, out abstractPopup))
				_popupCache.Remove(key);
			else
				abstractPopup = Instantiate(_popupPrefabsDictionary[key], _container);

			return abstractPopup;
		}

		public async UniTask<bool> ShowConfirmPopupAsync(string messageText  = "Are you sure?")
		{
			ConfirmPopup confirmPopup = InstantiateConfirmPopup(messageText);
			Popup abstractPopup = confirmPopup.AbstractPopup;

			_background.UnfadeAsync(Constants.PopupBackgroundFadeDuration).Forget();

			await UniTask.WhenAll(
				abstractPopup.UnfadeAsync(),
				abstractPopup.PlayShowAnimation()
			);


			bool isYesClicked = false;
			bool isNoClicked = false;

			EventHandler yesClickHandler = (s, a) => isYesClicked = true;
			EventHandler noClickHandler = (s, a) => isNoClicked = true;

			confirmPopup.YesButtonClicked += yesClickHandler;
			confirmPopup.NoButtonClicked += noClickHandler;

			await UniTask.WaitUntil(() => isYesClicked || isNoClicked);

			confirmPopup.YesButtonClicked -= yesClickHandler;
			confirmPopup.NoButtonClicked -= noClickHandler;


			if (_container.ActiveChildCount() == 1)
				_background.FadeAsync(Constants.PopupBackgroundFadeDuration).Forget();

			await UniTask.WhenAll(
				abstractPopup.FadeAsync(),
				abstractPopup.PlayHideAnimation()
			);

			return isYesClicked;
		}

	}
}
