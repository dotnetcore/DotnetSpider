using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 构造起始链接
	/// </summary>
	public interface IStartUrlsBuilder
	{
		void Build(Site spider);
	}

	public abstract class StartUrlsBuilder : Named, IStartUrlsBuilder
	{
		public abstract void Build(Site spider);
	}

	public class ForeachStartUrlsBuilder : StartUrlsBuilder
	{
		public int From { get; }

		public int To { get; }

		public int Interval { get; }

		public string Prefix { get; }

		public string Postfix { get; }

		public ForeachStartUrlsBuilder(int min, int max, int interval, string prefix, string postfix)
		{
			From = min;
			To = max;
			Interval = interval;
			Prefix = prefix;
			Postfix = postfix;
		}

		public override void Build(Site site)
		{
			for (int i = From; i <= To; i += Interval)
			{
				var request = new Request($"{Prefix}{i}{Postfix}");
				site.AddStartRequest(request);
			}
		}
	}

	public class ForeachDateStartUrlBuilder : StartUrlsBuilder
	{
		public DateTime From { get; }

		public DateTime To { get; }

		public int Interval { get; }

		public string DateFormateString { get; }

		public string Prefix { get; }

		public string Postfix { get; }

		public ForeachDateStartUrlBuilder(DateTime from, DateTime to, int interval, string prefix, string postfix, string dateFormateString = "yyyy-MM-dd")
		{
			From = from;
			To = to;
			Interval = interval;
			Prefix = prefix;
			Postfix = postfix;
			DateFormateString = dateFormateString;
		}

		public override void Build(Site site)
		{
			for (var i = From; i <= To; i = i.AddDays(Interval))
			{
				var date = i.ToString(DateFormateString);
				var request = new Request($"{Prefix}{date}{Postfix}");
				site.AddStartRequest(request);
			}
		}
	}
}
