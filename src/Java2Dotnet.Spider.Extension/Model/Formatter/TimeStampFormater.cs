using System;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TimeStampFormatter : Formatter
	{
		public override string Name { get; internal set; } = "TimeStampFormatter";

		public override string Formate(string value)
		{
			long timeStamp = long.Parse(value);
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0); ;
			switch (value.Length)
			{
				case 10:
					{
						dt = dt.AddSeconds(timeStamp).ToLocalTime();
						break;
					}
				case 13:
					{
						dt = dt.AddMilliseconds(timeStamp).ToLocalTime();
						break;
					}
				default:
					{
						throw new Exception("Wrong input timestamp");
					}
			}
			return dt.ToString("yyyy-MM-dd HH:mm:ss");
		}
	}
}
