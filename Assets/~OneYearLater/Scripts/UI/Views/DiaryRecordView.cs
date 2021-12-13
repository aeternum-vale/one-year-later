using System;
using TMPro;
using UnityEngine;

namespace OneYearLater.UI.Views
{
	public class DiaryRecordView : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _timeText;
		[SerializeField] private TextMeshProUGUI _contentText;
		[SerializeField] private Tapable _tapable;

		public int Id { get; set; }
		public string TimeText { get => _timeText.text; set => _timeText.text = value; }
		public string ContentText { get => _contentText.text; set => _contentText.text = value; }

		public event EventHandler LongTap;

		private void Awake()
		{
			_tapable.LongTap += OnLongTap;
		}

		private void OnDestroy()
		{
			_tapable.LongTap -= OnLongTap;
		}

		private void OnLongTap(object sender, EventArgs args) => LongTap?.Invoke(this, EventArgs.Empty);
	}
}
