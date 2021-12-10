using OneYearLater.Management.Interfaces;
using UnityEngine;
using Zenject;

namespace OneYearLater.Management
{
	public class App : MonoBehaviour
	{
		[Inject] private IScreensMediator _screensMediator;

		private void Start()
		{
			_screensMediator.InitializeScreens();
		}

	}
}
