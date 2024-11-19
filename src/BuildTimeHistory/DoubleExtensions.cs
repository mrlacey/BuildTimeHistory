using System;

namespace BuildTimeHistory
{
	public static class DoubleExtensions
	{
		public static string ToSmartTimeString(this double time)
		{
			if (time <= 0)
			{
				return "00:00";
			}

			var ts = TimeSpan.FromMilliseconds(time);

			if (ts.Hours > 0)
			{
				return $"{ts.Hours}:{ts.Minutes:00}:{ts.Seconds:00}";
			}
			else if (ts.Minutes > 0)
			{
				return $"{ts.Minutes}:{ts.Seconds:00}";
			}
			else
			{
				return $"00:{ts.Seconds:00}";
			}
		}
	}
}
