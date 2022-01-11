using System;
using System.Globalization;

namespace OneYearLater.Import
{
	public static class ImportUtils
	{
		public static bool IsDate(this string line, out DateTime date)
		{
			if (DateTime.TryParse(line, out date))
				return true;

			line = line.Replace("года", "");

			return DateTime.TryParse(line, out date);
		}
		public static bool IsDate(this string line) => IsDate(line, out DateTime _);


		public static bool IsTime(this string line, out DateTime date)
		{
			var trimmedLine = line.Trim();

			return DateTime.TryParseExact(trimmedLine, new[] { "HH:mm", "HH:mm:ss", "H:mm", "H:mm:ss" },
				CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
		}
		public static bool IsTime(this string line) => IsTime(line, out DateTime _);

	}
}