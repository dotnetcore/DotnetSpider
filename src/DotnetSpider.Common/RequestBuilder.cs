using System;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 起始链接构造器
	/// </summary>
	public interface IRequestBuilder
	{
		/// <summary>
		/// 构造起始链接对象并添加到网站信息对象中
		/// </summary>
		/// <param name="site">网站信息</param>
		void Build(Site site);
	}

	/// <summary>
	/// 起始链接构造器
	/// </summary>
	public abstract class RequestBuilder : Named, IRequestBuilder
	{
		/// <summary>
		/// 构造起始链接对象并添加到网站信息对象中
		/// </summary>
		/// <param name="site">网站信息</param>
		public abstract void Build(Site site);
	}

	/// <summary>
	/// 递增的起始链接构造器, 可以设置起始数字, 结束数字, 递增间隔, 链接前、后缀
	/// 如: From = 1, To = 10, Interval = 2, Prefix = www.baidu.com/, Postfix  = .html,
	/// 则最终可以构造出: www.baidu.com/1.html, www.baidu.com/3.html, www.baidu.com/5.html, www.baidu.com/7.html, www.baidu.com/9.html
	/// </summary>
	public class ForeachStartUrlsBuilder : RequestBuilder
	{
		/// <summary>
		/// 递增开始值
		/// </summary>
		public int From { get; }

		/// <summary>
		/// 递增结束值
		/// </summary>
		public int To { get; }

		/// <summary>
		/// 递增间隔
		/// </summary>
		public int Interval { get; }

		/// <summary>
		/// URL拼接前缀
		/// </summary>
		public string Prefix { get; }

		/// <summary>
		/// URL拼接后缀
		/// </summary>
		public string Postfix { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="min">递增开始值</param>
		/// <param name="max">递增结束值</param>
		/// <param name="interval">递增步进</param>
		/// <param name="prefix">URL拼接前缀</param>
		/// <param name="postfix">URL拼接后缀</param>
		public ForeachStartUrlsBuilder(int min, int max, int interval, string prefix, string postfix)
		{
			From = min;
			To = max;
			Interval = interval;
			Prefix = prefix;
			Postfix = postfix;
		}

		/// <summary>
		/// 构造起始链接对象并添加到网站信息对象中
		/// </summary>
		/// <param name="site">网站信息</param>
		public override void Build(Site site)
		{
			for (int i = From; i <= To; i += Interval)
			{
				var request = new Request($"{Prefix}{i}{Postfix}");
				site.AddRequests(request);
			}
		}
	}

	/// <summary>
	/// 递增时间的起始链接构造器, 可以设置起始时间, 结束时间, 时间格式化字符串, 递增间隔, 链接前、后缀
	/// 如: From = 2017-01-01, To = 2017-01-10, Interval = 1, Prefix = www.baidu.com/, Postfix  = .html, DateFormateString = yyyy-MM-dd
	/// 则最终可以构造出: www.baidu.com/2017-01-01.html, www.baidu.com/2017-01-02.html, www.baidu.com/2017-01-03.html...
	/// </summary>
	public class ForeachDateStartUrlBuilder : RequestBuilder
	{
		/// <summary>
		/// 递增起始时间
		/// </summary>
		public DateTime From { get; }

		/// <summary>
		/// 递增结束时间
		/// </summary>
		public DateTime To { get; }

		/// <summary>
		/// 递增间隔(天)
		/// </summary>
		public int Interval { get; }

		/// <summary>
		/// 时间格式化字符串
		/// </summary>
		public string DateFormateString { get; }

		/// <summary>
		/// URL拼接前缀
		/// </summary>
		public string Prefix { get; }

		/// <summary>
		/// URL拼接后缀
		/// </summary>
		public string Postfix { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="from">递增起始时间</param>
		/// <param name="to">递增结束时间</param>
		/// <param name="interval">递增间隔(天)</param>
		/// <param name="prefix">URL拼接前缀</param>
		/// <param name="postfix">URL拼接后缀</param>
		/// <param name="dateFormateString">时间格式化字符串</param>
		public ForeachDateStartUrlBuilder(DateTime from, DateTime to, int interval, string prefix, string postfix, string dateFormateString = "yyyy-MM-dd")
		{
			From = from;
			To = to;
			Interval = interval;
			Prefix = prefix;
			Postfix = postfix;
			DateFormateString = dateFormateString;
		}

		/// <summary>
		/// 构造起始链接对象并添加到网站信息对象中
		/// </summary>
		/// <param name="site">网站信息</param>
		public override void Build(Site site)
		{
			for (var i = From; i <= To; i = i.AddDays(Interval))
			{
				var date = i.ToString(DateFormateString);
				var request = new Request($"{Prefix}{date}{Postfix}");
				site.AddRequests(request);
			}
		}
	}
}
