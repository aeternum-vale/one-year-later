using Cysharp.Threading.Tasks;
using ExternalStorages;
using NaughtyAttributes;
using UnityEngine;

public class DropBoxTest : MonoBehaviour
{
	private DropBoxExternalStorage _dropBoxExternalStorage;
	[SerializeField] private string _accessCode;


	private void Start()
	{
		_dropBoxExternalStorage = new DropBoxExternalStorage();
	}

	[Button]
	public void RequestAccessCode()
	{
		_dropBoxExternalStorage.RequestAccessCode();
	}

	[Button]
	public void RequestToken()
	{
		_dropBoxExternalStorage.RequestToken(_accessCode).Forget();
	}

	[Button]
	public void RequestRefreshToken()
	{
		_dropBoxExternalStorage.RequestRefreshToken().Forget();
	}

	[Button]
	public void IsTokenValid()
	{
		_dropBoxExternalStorage.IsTokenValid()
			.ContinueWith(IsTokenValid => Debug.Log($"IsTokenValid={IsTokenValid}"))
			.Forget();

	}

}