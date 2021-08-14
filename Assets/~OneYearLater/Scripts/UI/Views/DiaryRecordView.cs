using TMPro;
using UnityEngine;

namespace OneYearLater.UI.Views
{
	public class DiaryRecordView : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _timeText;
		[SerializeField] private TextMeshProUGUI _contentText;

		public string TimeText { get => _timeText.text; set => _timeText.text = value; }
		public string ContentText { get => _contentText.text; set => _contentText.text = value; }
	}
}
