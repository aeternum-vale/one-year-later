using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Newtonsoft.Json;
using OneYearLater.Management.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

public class DropBoxExternalStorage : MonoBehaviour, IExternalStorage
{
	private const string AppKey = "x74srqkscwb6d3o";
	private const string AppSecret = "ywcxd32t1ohctwv";
	private const string LocalTestFilePath = @"C:\cookies.jpg";

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
		Dictionary<string, string> headers = new Dictionary<string, string>();

		headers.Add("code", _accessCode);
		headers.Add("grant_type", "authorization_code");
		headers.Add("client_id", AppKey);
		headers.Add("client_secret", AppSecret);

		PostForm("https://api.dropboxapi.com/oauth2/token", headers).ContinueWith((response) =>
		{
			string rawResult = response.Item1;
			bool isError = response.Item2;

			Debug.Log("rawResult=" + rawResult);

			Dictionary<string, string> result = rawResult.FromJsonToDictionary();

			Debug.Log("dictionary=");
			foreach (var kvp in result)
				Debug.Log($"{kvp.Key}:{kvp.Value}");


			if (isError)
				Debug.LogError(result);
			else
				_token = result["access_token"];
		})
		.Forget();
	}

	[Button]

	private async void UploadTestFile()
	{

		Dictionary<string, string> dropboxApiArg = new Dictionary<string, string>()
		{
			["path"] = "/" + Path.GetFileName(LocalTestFilePath),
			["mode"] = "overwrite",
			["autorename"] = "true",
			["mute"] = "false",
		};

		string dropboxApiArgsJson = dropboxApiArg.FromDictionaryToJson();
		Debug.Log(dropboxApiArgsJson);

		UnityWebRequest uwr = UnityWebRequest.Post("https://content.dropboxapi.com/2/files/upload", "");
		uwr.SetRequestHeader("Authorization", $"Bearer {_token}");
		uwr.SetRequestHeader("Dropbox-API-Arg", dropboxApiArgsJson);

		byte[] fileData = File.ReadAllBytes(LocalTestFilePath);

		uwr.uploadHandler = new UploadHandlerRaw(fileData) { contentType = "application/octet-stream" };
		await uwr.SendWebRequest().ToUniTask();

		if (uwr.result == UnityWebRequest.Result.ConnectionError)
			Debug.LogError(uwr.error);
		else
			Debug.Log("file uploaded!");
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

	UniTask IExternalStorage.Authenticate()
	{
		throw new System.NotImplementedException();
	}

	UniTask IExternalStorage.Sync()
	{
		throw new System.NotImplementedException();
	}
}

public static class Extensions
{
	public static string FromDictionaryToJson(this Dictionary<string, string> dictionary)
	{
		var kvs = dictionary.Select(kvp =>
				string.Format("\"{0}\":{1}",
				kvp.Key,
				bool.TryParse(kvp.Value, out bool parsedBool) ? kvp.Value : $"\"{kvp.Value}\"")
			);

		return string.Concat("{", string.Join(",", kvs), "}");
	}

	public static Dictionary<string, string> FromJsonToDictionary(this string json)
	{
		string[] keyValueArray = json.Replace("{", string.Empty).Replace("}", string.Empty).Replace("\"", string.Empty).Split(',');
		return keyValueArray.ToDictionary(item => item.Split(':')[0].Trim(), item => item.Split(':')[1].Trim());
	}
}