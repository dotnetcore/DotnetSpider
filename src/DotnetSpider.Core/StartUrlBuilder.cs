using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Some easy method to help spider build start urls.
	/// </summary>
	public interface IStartUrlBuilder
	{
		void Build(Site spider);
	}

	public abstract class StartUrlBuilder : Named, IStartUrlBuilder
	{
		public abstract void Build(Site spider);
	}

	public sealed class CycleStartUrlBuilder : StartUrlBuilder
	{
		public int From { get; }

		public int To { get; }

		public int Interval { get; }

		public string Prefix { get; }

		public string Postfix { get; }

		public CycleStartUrlBuilder(int min, int max, int interval, string prefix, string postfix)
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

	public sealed class CycleDateStartUrlBuilder : StartUrlBuilder
	{
		public DateTime From { get; }

		public DateTime To { get; }

		public int Interval { get; }

		public string DateFormateString { get; }

		public string Prefix { get; }

		public string Postfix { get; }

		public CycleDateStartUrlBuilder(DateTime from, DateTime to, int interval, string prefix, string postfix, string dateFormateString = "yyyy-MM-dd")
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
