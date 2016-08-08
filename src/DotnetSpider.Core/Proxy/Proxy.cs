using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core.Common;

namespace DotnetSpider.Core.Proxy
{
	public class Proxy
	{
		public const int Error403 = 403;
		public const int Error404 = 404;
		public const int ErrorBanned = 10000;
		public const int ErrorProxy = 10001;
		public const int Success = 200;

		private readonly HttpHost _httpHost;

		private int _reuseTimeInterval = 1500;// ms
		private double _canReuseTime = 0L;
		private double _lastBorrowTime = DateTimeUtils.GetCurrentTimeStamp();
		private double _responseTime = 0L;
		private long _idleTime = 0L;

		private int _failedNum = 0;
		private int _successNum = 0;
		private int _borrowNum = 0;

		private IList<int> _failedErrorType = new List<int>();

		public Proxy(HttpHost httpHost)
		{
			_httpHost = httpHost;

			_canReuseTime = DateTimeUtils.GetCurrentTimeStamp() + _reuseTimeInterval * 100;
		}

		public int GetSuccessNum()
		{
			return _successNum;
		}

		public void SuccessNumIncrement(int increment)
		{
			_successNum += increment;
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
			_responseTime = (DateTimeUtils.GetCurrentTimeStamp() - _lastBorrowTime + _responseTime) / 2;
			_lastBorrowTime = DateTimeUtils.GetCurrentTimeStamp();
		}

		public IList<int> GetFailedErrorType()
		{
			return _failedErrorType;
		}

		public void SetFailedErrorType(List<int> failedErrorType)
		{
			_failedErrorType = failedErrorType;
		}

		public void Fail(int failedErrorType)
		{
			_failedNum++;
			_failedErrorType.Add(failedErrorType);
		}

		public void SetFailedNum(int failedNum)
		{
			_failedNum = failedNum;
		}

		public int FailedNum { get; set; }
 

		public string GetFailedType()
		{
			return _failedErrorType.Aggregate("", (current, i) => current + (i + " . "));
		}

		public HttpHost GetHttpHost()
		{
			return _httpHost;
		}

		public int GetReuseTimeInterval()
		{
			return _reuseTimeInterval;
		}

		public void SetReuseTimeInterval(int reuseTimeInterval)
		{
			_reuseTimeInterval = reuseTimeInterval;
			_canReuseTime = DateTimeUtils.GetCurrentTimeStamp() + reuseTimeInterval * 100;
		}

		//public long getDelay(TimeUnit unit) {
		//	return unit.convert(canReuseTime - System.nanoTime(), unit.NANOSECONDS);
		//}

		//public int compareTo(Delayed o) {
		//	Proxy that = (Proxy) o;
		//	return canReuseTime > that.canReuseTime ? 1 : (canReuseTime < that.canReuseTime ? -1 : 0);
		//}

		public override string ToString()
		{
			string re = $"host: {_httpHost} >> {_responseTime} >> success: {_successNum*100.0/_borrowNum} >> borrow: %d";
			return re;
		}

		public void BorrowNumIncrement(int increment)
		{
			_borrowNum += increment;
		}

		public int GetBorrowNum()
		{
			return _borrowNum;
		}
	}
}