using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using Utilities;

namespace OneYearLater.UI.Popups
{
	public class PopupManager : MonoBehaviour, IPopupManager
	{
		[SerializeField] private PopupSPair[] _popupPrefabsArray;
		private Dictionary<EPopupKey, Popup> _popupPrefabsDictionary;
		private Dictionary<EPopupKey, Popup> _popupCache = new Dictionary<EPopupKey, Popup>();

		[SerializeField] private CanvasGroupFader _background;
		[SerializeField] private Transform _container;

		public bool IsAnyPopupActive => _container.ActiveChildCount() > 0;


		#region Unity Callbacks
		private void Awake()
		{
			_popupPrefabsDictionary = _popupPrefabsArray.ToDictionary();
		}

		#endregion

		public UniTask RunMessagePopupAsync(string messageText)
		{
			return RunMessagePopupAsync(messageText, "OK");
		}

		public async UniTask RunMessagePopupAsync(string messageText, string okButtonText)
		{
			MessagePopup messagePopup = GetPopupInstance<MessagePopup>(EPopupKey.Message);
			messagePopup.Init(messageText, okButtonText);
			await RunSpecificPopupAsync(messagePopup);
		}

		public async UniTask<string> RunPromptPopupAsync(string messageText, string okButtonText = "OK", string placeholderText = "")
		{
			PromptPopup promptPopup = GetPopupInstance<PromptPopup>(EPopupKey.Prompt);
			promptPopup.Init(messageText, okButtonText, placeholderText);
			await RunSpecificPopupAsync(promptPopup);
			return promptPopup.InputFieldText;
		}

		public async UniTask<bool> RunConfirmPopupAsync(string messageText = "Are you sure?")
		{
			ConfirmPopup confirmPopup = GetPopupInstance<ConfirmPopup>(EPopupKey.Confirm);
			confirmPopup.Init(messageText);
			await RunSpecificPopupAsync(confirmPopup);
			return confirmPopup.Answer;
		}

		private async UniTask RunSpecificPopupAsync<T>(T specificPopup) where T : ISpecificPopup
		{
			var abstractPopup = specificPopup.AbstractPopup;

			_background.UnfadeAsync(Constants.PopupBackgroundFadeDuration).Forget();

			await abstractPopup.ShowAsync();
			await specificPopup.WaitForUserAnswerAsync();

			if (_container.ActiveChildCount() == 1)
				_background.FadeAsync(Constants.PopupBackgroundFadeDuration).Forget();

			await abstractPopup.HideAsync();
			_popupCache[specificPopup.Key] = abstractPopup;
		}

		private T GetPopupInstance<T>(EPopupKey key) where T : ISpecificPopup
		{
			Popup abstractPopup = GetPopupInstance(key);
			var specificPopup = abstractPopup.GetComponent<T>();
			return specificPopup;
		}

		private Popup GetPopupInstance(EPopupKey key)
		{
			Popup abstractPopup;

			if (_popupCache.TryGetValue(key, out abstractPopup))
				_popupCache.Remove(key);
			else
				abstractPopup = Instantiate(_popupPrefabsDictionary[key], _container);

			return abstractPopup;
		}
	}
}
