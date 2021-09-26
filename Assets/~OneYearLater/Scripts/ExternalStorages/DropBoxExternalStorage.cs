using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using Utilities;

using Debug = UnityEngine.Debug;

namespace ExternalStorages
{
	public class DropBoxExternalStorage : IExternalStorage
	{
		private struct DropBoxState
		{
			public string token;
			public string refreshToken;
			public string codeVerifier;
			public string codeChallenge;
		}

		public EExternalStorageKey Key => EExternalStorageKey.DropBox;
		public string Name => "DropBox";
		public ReactiveProperty<string> PersistentState { get; private set; }

		private DropBoxState _state;
		private string _appKey = "x74srqkscwb6d3o"; // aka client_id

		public void Init(string state)
		{
			try
			{
				_state = JsonConvert.DeserializeObject<DropBoxState>(state);
			}
			catch
			{
				_state = new DropBoxState();
			}

			PersistentState = new ReactiveProperty<string>();
			UpdatePersistentState();
		}

		private void UpdatePersistentState()
		{
			PersistentState.Value = JsonConvert.SerializeObject(_state);
		}

		private string GeneratePKCECodeVerifier()
		{
			var bytes = new byte[128];
			RandomNumberGenerator.Create().GetBytes(bytes);
			return Convert.ToBase64String(bytes)
				.TrimEnd('=')
				.Replace('+', '-')
				.Replace('/', '_')
				.Substring(0, 128);
		}

		private string GeneratePKCECodeChallenge(string codeVerifier)
		{
			using (var sha256 = SHA256.Create())
			{
				var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
				return Convert.ToBase64String(challengeBytes)
					.TrimEnd('=')
					.Replace('+', '-')
					.Replace('/', '_');
			}
		}

		public async UniTask<bool> IsTokenValid()
		{

			Dictionary<string, object> dropboxArgs = new Dictionary<string, object>() { ["jack"] = "black", };

			string dropboxArgsJson = JsonConvert.SerializeObject(dropboxArgs);
			UnityWebRequest uwr = UnityWebRequest.Post("https://api.dropboxapi.com/2/check/user", "");
			uwr.SetRequestHeader("Authorization", $"Bearer {_state.token}");
			uwr.SetRequestHeader("Content-Type", "application/json");

			uwr.uploadHandler = new UploadHandlerRaw(Encoding.ASCII.GetBytes(dropboxArgsJson));

			try
			{
				await uwr.SendWebRequest().ToUniTask();
				return uwr.result == UnityWebRequest.Result.Success;
			}

			catch
			{
				return false;
			}
		}

		public void RequestAccessCode()
		{
			_state.codeVerifier = GeneratePKCECodeVerifier();
			_state.codeChallenge = GeneratePKCECodeChallenge(_state.codeVerifier);
			UpdatePersistentState();

			Application.OpenURL($"https://www.dropbox.com/oauth2/authorize?"
				+ $"client_id={_appKey}&"
				+ "response_type=code&"
				+ "token_access_type=offline&"
				+ $"code_challenge={_state.codeChallenge}&"
				+ "code_challenge_method=S256");
		}

		public async UniTask<bool> RequestToken(string accessCode)
		{
			Debug.Log($"{GetType()}:{nameof(RequestToken)}");
		 	
			Dictionary<string, string> formFields = new Dictionary<string, string>();

			formFields.Add("code", accessCode);
			formFields.Add("grant_type", "authorization_code");
			formFields.Add("code_verifier", _state.codeVerifier);
			formFields.Add("client_id", _appKey);

			(string, Exception) response = await PostForm("https://api.dropboxapi.com/oauth2/token", formFields);

			if (response.Item2 != null)
			{
				Debug.LogError(response.Item2);
				return false;
			}

			Debug.Log("rawResult=" + response.Item1);
			var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Item1);

			Debug.Log("dictionary=");
			foreach (var kvp in result)
				Debug.Log($"{kvp.Key}:{kvp.Value}");

			_state.refreshToken = result["refresh_token"];
			_state.token = result["access_token"];
			UpdatePersistentState();

			return true;
		}

		public async UniTask<bool> RequestRefreshToken()
		{
			Debug.Log($"{GetType()}:{nameof(RequestRefreshToken)}");

			Dictionary<string, string> formFields = new Dictionary<string, string>();

			formFields.Add("grant_type", "refresh_token");
			formFields.Add("refresh_token", _state.refreshToken);
			formFields.Add("client_id", _appKey);

			(string, Exception) response = await PostForm("https://api.dropboxapi.com/oauth2/token", formFields);
			if (response.Item2 != null)
			{
				Debug.LogError(response.Item2);
				return false;
			}

			Debug.Log("rawResult=" + response.Item1);

			var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Item1);

			Debug.Log("dictionary=");
			foreach (var kvp in result)
				Debug.Log($"{kvp.Key}:{kvp.Value}");

			_state.token = result["access_token"];
			UpdatePersistentState();

			return true;
		}

		public async UniTask DownloadFile(string externalStoragePath, string localStoragePath)
		{
			Dictionary<string, string> dropboxApiArg = new Dictionary<string, string>() { ["path"] = externalStoragePath };

			string dropboxApiArgsJson = JsonConvert.SerializeObject(dropboxApiArg);
			Debug.Log(dropboxApiArgsJson);

			UnityWebRequest uwr = UnityWebRequest.Post("https://content.dropboxapi.com/2/files/download", "");
			uwr.SetRequestHeader("Authorization", $"Bearer {_state.token}");
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
			uwr.SetRequestHeader("Authorization", $"Bearer {_state.token}");
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
			uwr.SetRequestHeader("Authorization", $"Bearer {_state.token}");
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

		private async UniTask<(string, Exception)> PostForm(string url, Dictionary<string, string> formFields)
		{
			UnityWebRequest uwr = UnityWebRequest.Post(url, formFields);

			try
			{
				await uwr.SendWebRequest().ToUniTask();
			}
			catch (Exception e)
			{
				return (null, e);
			}

			if (uwr.result == UnityWebRequest.Result.ConnectionError)
				return (null, new Exception(uwr.error));
			else
				return (uwr.downloadHandler.text, null);
		}


		public UniTask<bool> Connect(string code)
		{
			return RequestToken(code);
		}

		public async UniTask<bool> IsConnected()
		{
			return await IsTokenValid() || await RequestRefreshToken();
		}
	}
}
