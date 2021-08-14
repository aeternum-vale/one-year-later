using TMPro;
using UnityEngine;

namespace OneYearLater.UI.Views
{
	public class SettingParameterView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _textComponent;

		public string Text
		{
			get => _textComponent.text;
			set => _textComponent.text = value;
		}

	}
}
