using System;
using Cysharp.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
	public interface IScreensMediator
	{
		UniTask InitializeScreens();
		UniTask ActivateExternalStoragesScreens();

		UniTask ActivateRecordEditorScreenInBlankMode();
		UniTask ActivateRecordEditorScreen(string recordHash);
		UniTask ActivateFeedScreen();
		UniTask ActivateFeedScreenForToday();
		UniTask ActivateFeedScreenFor(DateTime date);


	}
}