
using Cysharp.Threading.Tasks;
using OneYearLater.Management.Interfaces.Importers;
using UnityEngine;

namespace OneYearLater.Import
{
	public class ConversationImporter : TextFileImporter, IConversationImporter
	{
		protected override UniTask Parse(byte[] bytes)
		{
			foreach (string line in GetLines(bytes))
			{
				Debug.Log($"<color=lightblue>{GetType().Name}:</color> line={line}");
			}

			return UniTask.CompletedTask;
		}
	}
}