
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.Interfaces.Importers;
using OneYearLater.Management.ViewModels;
using UnityEngine;
using Zenject;

namespace OneYearLater.Import
{
	public class ConversationImporter : TextFileImporter, IConversationImporter
	{
		[Inject] private IPopupManager _popupManager;
		private readonly TimeSpan _messagesGluingInterval = new TimeSpan(0, 10, 0);


		protected async override UniTask Parse(byte[] bytes)
		{
			string importPattern =
				await _popupManager.RunPromptPopupAsync("Enter pattern", "e.g. {author(Your Name, Companion Name, Friendly Companion Name)} [{date}]");

			string dateExpression = "{date}";

			string authorRawRxs = @"\b[\w ]+\b";
			string userRxGroupName = "user";
			string companionRxGroupName = "companion";
			string friendlyCompanionRxGroupName = "friendlycompanion";

			string importPatternAuthorRxs = @$"author\((?<{userRxGroupName}>{authorRawRxs}), *(?<{companionRxGroupName}>{authorRawRxs}), *(?<{friendlyCompanionRxGroupName}>{authorRawRxs})\)";
			importPatternAuthorRxs = "{" + importPatternAuthorRxs + "}";

			Debug.Log($"<color=lightblue>{GetType().Name}:</color> importPatternAuthorRxs={importPatternAuthorRxs}");
			Regex importPatternAuthorRx = new Regex(importPatternAuthorRxs, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			Match authorMatch = importPatternAuthorRx.Match(importPattern);

			if (!authorMatch.Success)
				throw new Exception("invalid author expression"); //TODO must be custom

			string userName = authorMatch.Groups[userRxGroupName].Value;
			string companionName = authorMatch.Groups[companionRxGroupName].Value;
			string friendlyCompanionName = authorMatch.Groups[friendlyCompanionRxGroupName].Value;

			int dateIndex = importPattern.IndexOf(dateExpression);
			if (dateIndex == -1)
				throw new Exception("invalid date experssion");

			string authorHeaderExpression = ":A:U:T::H:O:R:";
			string dateHeaderExpression = ":D:A::T:E:";
			string authorHeaderRxGroupName = "author";
			string authorHeaderRxs = @$"(?<{authorHeaderRxGroupName}>({userName})|({companionName}))";
			string dateHeaderRxGroupName = "date";
			string dateHeaderRxs = @$"(?<{dateHeaderRxGroupName}>[\w:. \\/+-]+)";

			string headerRxs = importPattern;
			headerRxs = Regex.Replace(headerRxs, importPatternAuthorRxs, authorHeaderExpression);
			headerRxs = headerRxs.Replace(dateExpression, dateHeaderExpression);
			headerRxs = Regex.Escape(headerRxs);
			headerRxs = headerRxs.Replace(authorHeaderExpression, authorHeaderRxs);
			headerRxs = headerRxs.Replace(dateHeaderExpression, dateHeaderRxs);

			var headerRx = new Regex(headerRxs, RegexOptions.Compiled | RegexOptions.IgnoreCase);

			MessageRecordViewModel currentMessage = null;
			var messageTextBuilder = new StringBuilder();
			string currentMessageRawAuthorName = string.Empty;
			DateTime lastMessageDateTime = new DateTime();
			foreach (string line in GetLines(bytes))
			{
				string debugLine = (line.Length < 50) ? line : (line.Substring(0, 50) + "...");
				Debug.Log($"<color=lightblue>{GetType().Name}:</color> checking line '{debugLine}'");

				Match headerMatch = headerRx.Match(line);
				if (headerMatch.Success)
				{
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{line}' matches {headerRxs} pattern!");
					string author = headerMatch.Groups[authorHeaderRxGroupName].Value;
					string dateStr = headerMatch.Groups[dateHeaderRxGroupName].Value;
					if (!dateStr.IsDate(out DateTime parsedDate))
						throw new Exception("invalid date");


					if (currentMessage != null &&
					author.Equals(currentMessageRawAuthorName) &&
					parsedDate.Subtract(lastMessageDateTime) <= _messagesGluingInterval)
					{
						Debug.Log($"<color=lightblue>{GetType().Name}:</color> gluing line '{line}' with previous");
						lastMessageDateTime = parsedDate;
						continue;
					}

					if (messageTextBuilder.Length > 0 && currentMessage != null)
					{
						currentMessage.MessageText = messageTextBuilder.ToString().Trim();
						await InsertNewRecordToDb(currentMessage);
					}

					Debug.Log($"<color=lightblue>{GetType().Name}:</color> author={author}");
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> date={dateStr}");

					bool isFromUser = userName.Equals(author);

					currentMessage = new MessageRecordViewModel(parsedDate);

					currentMessage.IsFromUser = isFromUser;
					currentMessage.ConversationalistName = friendlyCompanionName;
					currentMessageRawAuthorName = author;
					lastMessageDateTime = parsedDate;

					messageTextBuilder.Clear();
				}
				else
				{
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{debugLine}' do not matches {headerRxs} pattern, it is simple text");

					messageTextBuilder.AppendLine(line);
				}
			}
			currentMessage.MessageText = messageTextBuilder.ToString().Trim();
			await InsertNewRecordToDb(currentMessage);
		}
	}
}