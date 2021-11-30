using Keiwando.NFSO;
using UnityEngine;

namespace OneYearLater.Management
{
	public class Importer
	{
		public void ImportFromTextFile()
		{
			NativeFileSO.shared.OpenFile(
				new SupportedFileType[] { SupportedFileType.PlainText },
				(isOpened, file) =>
				{
					if (isOpened)
					{
						Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnImportFromTxtButtonClick file.Name={file.Name}");

						Debug.Log($"<color=lightblue>{GetType().Name}:</color> content={file.ToUTF8String()}");
					}
					else
					{
						Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnImportFromTxtButtonClick file dialog dismissed");
					}
				});
		}
	}

}