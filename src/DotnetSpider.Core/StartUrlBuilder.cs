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
		public int Min { get; }

		public int Max { get; }

		public int Interval { get; }

		public string Prefix { get; }

		public string Postfix { get; }

		public CycleStartUrlBuilder(int min, int max, int interval, string prefix, string postfix)
		{
			Min = min;
			Max = max;
			Interval = interval;
			Prefix = prefix;
			Postfix = postfix;
		}

		public override void Build(Site site)
		{
			for (int i = Min; i <= Max; i += Interval)
			{
				var request = new Request($"{Prefix}{i}{Postfix}");
				site.AddStartRequest(request);
			}
		}
	}

	public sealed class CycleDateStartUrlBuilder : StartUrlBuilder
	{
		public DateTime Min { get; }

		public DateTime Max { get; }

		public int IntervalDay { get; }

		public string DateFormateString { get; }

		public string Prefix { get; }

		public string Postfix { get; }


		public CycleDateStartUrlBuilder(DateTime min, DateTime max, int interval, string prefix, string postfix, string dateFormateString = "yyyy-MM-dd")
		{
			Min = min;
			Max = max;
			IntervalDay = interval;
			Prefix = prefix;
			Postfix = postfix;
			DateFormateString = dateFormateString;
		}

		public override void Build(Site site)
		{
			for (var i = Min; i <= Max; i = i.AddDays(IntervalDay))
			{
				var date = i.ToString(DateFormateString);
				var request = new Request($"{Prefix}{date}{Postfix}");
				site.AddStartRequest(request);
			}
		}
	}
}
