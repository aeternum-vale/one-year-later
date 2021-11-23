using UnityEngine;

namespace Utilities
{
	public class DevelopmentEntity : MonoBehaviour
	{
		[SerializeField] private bool _editor;
		[SerializeField] private bool _developmentBuild;

		private void Awake()
		{
			bool disable = true;

#if UNITY_EDITOR
			if (_editor)
				disable = false;
#endif

#if DEVELOPMENT_BUILD
			if (_developmentBuild)
				disable = false;
#endif

			gameObject.SetActive(!disable);
		}

	}
}