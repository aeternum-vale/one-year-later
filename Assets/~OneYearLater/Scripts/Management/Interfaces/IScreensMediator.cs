using System;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IScreensMediator
	{
		UniTask InitializeScreens();
		UniTask ActivateExternalStoragesScreens();

		UniTask ActivateRecordEditorScreenInBlankMode();
		UniTask ActivateRecordEditorScreen(int recordId);
		UniTask ActivateFeedScreenForToday();
		UniTask ActivateFeedScreen(DateTime date);


	}
}