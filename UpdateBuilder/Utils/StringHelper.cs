using System;

namespace UpdateBuilder.Utils
{
	public static class StringHelper
	{
		public static string BytesToString(this long byteCount)
		{
			string[] array = new string[7]
			{
				"B",
				"KB",
				"MB",
				"GB",
				"TB",
				"PB",
				"EB"
			};
			if (byteCount == 0)
			{
				return "0" + array[0];
			}
			long num = Math.Abs(byteCount);
			int num2 = Convert.ToInt32(Math.Floor(Math.Log(num, 1024.0)));
			double num3 = Math.Round((double)num / Math.Pow(1024.0, num2), 1);
			return (double)Math.Sign(byteCount) * num3 + array[num2];
		}
	}
}
