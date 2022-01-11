using System;
using System.Text;
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Exceptions;
using OneYearLater.Management.Interfaces.Importers;
using OneYearLater.Management.ViewModels;
using UniRx;
using UnityEngine;

namespace OneYearLater.Import
{
	public class DiaryImporter : TextFileImporter, IDiaryImporter
	{
		protected override async UniTask Parse(byte[] bytes)
		{
			DateTime currentDateTime = DateTime.MinValue;
			var recordTextSB = new StringBuilder();

			foreach (string line in GetLines(bytes))
			{
				if (line.IsDate(out DateTime parsedDate))
				{
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{line}' is a date!");

					if (recordTextSB.Length > 0)
						await InsertNewRecordToDb(currentDateTime, recordTextSB.ToString());

					if (line.IsTime(out DateTime parsedOnlyTime))
					{
						Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{line}' is a time!");

						if (currentDateTime == DateTime.MinValue)
							throw new ImportException("Time line before any date line");

						DateTime c = currentDateTime;
						DateTime t = parsedOnlyTime;

						currentDateTime = new DateTime(c.Year, c.Month, c.Day, t.Hour, t.Minute, t.Second);
					}
					else
						currentDateTime = parsedDate;

					recordTextSB = recordTextSB.Clear();
				}
				else
				{
					string debugLine = (line.Length < 50) ? line : (line.Substring(0, 50) + "...");
					Debug.Log($"<color=lightblue>{GetType().Name}:</color> line '{debugLine}' is not a date");
					recordTextSB.AppendLine(line);
				}
			}

			await InsertNewRecordToDb(currentDateTime, recordTextSB.ToString());
		}

		private UniTask InsertNewRecordToDb(DateTime dateTime, string text)
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> creating new record with date={dateTime} and text='{text}'");

			text = text.Trim();

			var diaryVM = new DiaryRecordViewModel(dateTime, text);

			return InsertNewRecordToDb(diaryVM);
		}
	}

}