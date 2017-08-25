using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public interface IStartUrlBuilder
	{
		void Build(Site spider);
	}

	public abstract class StartUrlBuilder : Named, IStartUrlBuilder
	{
		public abstract void Build(Site spider);
	}

	public class CycleStartUrlBuilder : StartUrlBuilder
	{
		public int Min { get; private set; }
		public int Max { get; private set; }
		public int Interval { get; private set; }

		public string Prefix { get; private set; }

		public string Postfix { get; private set; }

		public CycleStartUrlBuilder(int min, int max, int interval, string prefix, string postfix)
		{
			Min = min;
			Max = max;
			Interval = interval;
			Prefix = prefix;
			Postfix = postfix;
		}

		protected virtual void FormateRequest(Request request)
		{
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

	public class CycleDateStartUrlBuilder : StartUrlBuilder
	{
		public DateTime Min { get; private set; }

		public DateTime Max { get; private set; }

		public int IntervalDay { get; private set; }

		public string DateFormateString { get; private set; }

		public string Prefix { get; private set; }

		public string Postfix { get; private set; }

		protected virtual void FormateRequest(Request request)
		{
		}

		public CycleDateStartUrlBuilder(DateTime min, DateTime max, int interval, string prefix, string postfix, string dateFormateString = "yyyy-MM-dd")
		{
			Min = min;
			Max = max;
			IntervalDay = interval;
			Prefix = prefix;
			Postfix = postfix;
		}

		public override void Build(Site site)
		{
			for (var i = Min; i <= Max; i = i.AddDays(IntervalDay))
			{
				var date = i.ToString(DateFormateString);
				var request = new Request($"{Prefix}{date}{Postfix}");
				FormateRequest(request);
				site.AddStartRequest(request);
			}
		}
	}
}
