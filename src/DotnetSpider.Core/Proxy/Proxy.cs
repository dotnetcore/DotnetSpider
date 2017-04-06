using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Proxy
{
	public class Proxy
	{
		private double _lastBorrowTime = DateTimeUtils.GetCurrentTimeStamp();

		public Proxy(UseSpecifiedUriWebProxy httpHost, int reuseTimeInterval = 1500)
		{
			HttpHost = httpHost;

			CanReuseTime = DateTimeUtils.GetCurrentTimeStamp() + reuseTimeInterval * 100;
		}

		public double GetLastUseTime()
		{
			return _lastBorrowTime;
		}

		public void SetLastBorrowTime(double lastBorrowTime)
		{
			_lastBorrowTime = lastBorrowTime;
		}

		public void RecordResponse()
		{
			ResponseTime = (DateTimeUtils.GetCurrentTimeStamp() - _lastBorrowTime + ResponseTime) / 2;
			_lastBorrowTime = DateTimeUtils.GetCurrentTimeStamp();
		}

		public void Fail()
		{
			FailedNum++;
		}

		public readonly UseSpecifiedUriWebProxy HttpHost;

		public double CanReuseTime { get; set; }

		public double ResponseTime { get; private set; }

		public int FailedNum { get; private set; }

		public UseSpecifiedUriWebProxy GetWebProxy()
		{
			return HttpHost;
		}

		public void SetFailedNum(int num)
		{
			FailedNum = num;
		}

		public void SetReuseTime(int reuseTimeInterval)
		{
			CanReuseTime = DateTimeUtils.GetCurrentTimeStamp() + reuseTimeInterval * 100;
		}
	}
}