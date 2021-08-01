using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Newtonsoft.Json;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

using Debug = UnityEngine.Debug;

public class DropBoxExternalStorage : MonoBehaviour, IExternalStorage
{
	private const string AppKey = "x74srqkscwb6d3o";
	private const string AppSecret = "ywcxd32t1ohctwv";

	[SerializeField] private string _accessCode;
	[SerializeField] private string _token;

	[Button]
	public void RequestAccessCode()
	{
		Application.OpenURL($"https://www.dropbox.com/oauth2/authorize?client_id={AppKey}&response_type=code");
	}

	[Button]
	public void RequestToken()
	{
		RequestToken(_accessCode).Forget();
	}

	public async UniTask RequestToken(string accessCode)
	{
		Dictionary<string, string> formFields = new Dictionary<string, string>();

		formFields.Add("code", accessCode);
		formFields.Add("grant_type", "authorization_code");
		formFields.Add("client_id", AppKey);
		formFields.Add("client_secret", AppSecret);

		var response = await PostForm("https://api.dropboxapi.com/oauth2/token", formFields);

		string rawResult = response.Item1;
		bool isError = response.Item2;

		Debug.Log("rawResult=" + rawResult);

		var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawResult);

		Debug.Log("dictionary=");
		foreach (var kvp in result)
			Debug.Log($"{kvp.Key}:{kvp.Value}");

		if (isError)
			Debug.LogError(result);
		else
			_token = result["access_token"];
	}

	public async UniTask DownloadFile(string externalStoragePath, string localStoragePath)
	{
		Dictionary<string, string> dropboxApiArg = new Dictionary<string, string>() { ["path"] = externalStoragePath };

		string dropboxApiArgsJson = JsonConvert.SerializeObject(dropboxApiArg);
		Debug.Log(dropboxApiArgsJson);

		UnityWebRequest uwr = UnityWebRequest.Post("https://content.dropboxapi.com/2/files/download", "");
		uwr.SetRequestHeader("Authorization", $"Bearer {_token}");
		uwr.SetRequestHeader("Dropbox-API-Arg", dropboxApiArgsJson);
		uwr.SetRequestHeader("Content-Type", "application/octet-stream");

		uwr.downloadHandler = new DownloadHandlerBuffer();
		await uwr.SendWebRequest().ToUniTask();

		if (uwr.result == UnityWebRequest.Result.ConnectionError)
			Debug.LogError(uwr.error);
		else
		{
			File.WriteAllBytes(localStoragePath, uwr.downloadHandler.data);
		}
	}

	public async UniTask UploadFile(string localStoragePath, string externalStoragePath)
	{
		Dictionary<string, object> dropboxApiArg = new Dictionary<string, object>()
		{
			["path"] = externalStoragePath,
			["mode"] = "overwrite",
			["autorename"] = true,
			["mute"] = false,
		};

		string dropboxApiArgsJson = JsonConvert.SerializeObject(dropboxApiArg);
		Debug.Log(dropboxApiArgsJson);

		UnityWebRequest uwr = UnityWebRequest.Post("https://content.dropboxapi.com/2/files/upload", "");
		uwr.SetRequestHeader("Authorization", $"Bearer {_token}");
		uwr.SetRequestHeader("Dropbox-API-Arg", dropboxApiArgsJson);
		uwr.SetRequestHeader("Content-Type", "application/octet-stream");

		byte[] fileData = File.ReadAllBytes(localStoragePath);

		uwr.uploadHandler = new UploadHandlerRaw(fileData);
		await uwr.SendWebRequest().ToUniTask();

		if (uwr.result == UnityWebRequest.Result.ConnectionError)
			Debug.LogError(uwr.error);
	}

	public async UniTask<bool> IsFileExist(string path)
	{
		Dictionary<string, object> dropboxApiArg = new Dictionary<string, object>()
		{
			["path"] = path,
			["include_media_info"] = false,
			["include_deleted"] = false,
			["include_has_explicit_shared_members"] = false
		};
		string dropboxApiArgsJson = JsonConvert.SerializeObject(dropboxApiArg);
		UnityWebRequest uwr = UnityWebRequest.Post("https://api.dropboxapi.com/2/files/get_metadata", "");
		uwr.SetRequestHeader("Authorization", $"Bearer {_token}");
		uwr.SetRequestHeader("Content-Type", "application/json");
		uwr.uploadHandler = new UploadHandlerRaw(Encoding.ASCII.GetBytes(dropboxApiArgsJson));

		uwr.downloadHandler = new DownloadHandlerBuffer();

		try
		{
			await uwr.SendWebRequest().ToUniTask();
		}
		catch (UnityWebRequestException e)
		{
			if (uwr.responseCode != 409)
				throw e;
		}

		return uwr.result == UnityWebRequest.Result.Success;
	}

	private async UniTask<(string, bool)> PostForm(string url, Dictionary<string, string> formFields)
	{
		UnityWebRequest uwr = UnityWebRequest.Post(url, formFields);

		await uwr.SendWebRequest().ToUniTask();

		if (uwr.result == UnityWebRequest.Result.ConnectionError)
			return (uwr.error, true);
		else
			return (uwr.downloadHandler.text, false);
	}

	public UniTask Authenticate()
	{
		throw new System.NotImplementedException();
	}

	public UniTask Sync()
	{
		throw new System.NotImplementedException();
	}
}
