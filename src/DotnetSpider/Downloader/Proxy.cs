using System.Net;
using DotnetSpider.Core;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 代理信息
	/// </summary>
	public class Proxy
	{
		private double _lastBorrowTime = DateTimeHelper.GetCurrentUnixTimeNumber();

		/// <summary>
		/// 实际代理信息
		/// </summary>
		public readonly WebProxy WebProxy;

		/// <summary>
		/// 下一次可使用的时间
		/// </summary>
		public double CanReuseTime;

		/// <summary>
		/// 通过代理完成一次下载操作消耗的时间
		/// </summary>
		public double ResponseTime { get; private set; }

		/// <summary>
		/// 使用此代理下载数据的失败次数
		/// </summary>
		public int FailedNum { get; private set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="proxy">实际代理信息</param>
		/// <param name="reuseTimeInterval">代理不被再次使用的间隔</param>
		public Proxy(WebProxy proxy, int reuseTimeInterval = 1500)
		{
			WebProxy = proxy;

			CanReuseTime = DateTimeHelper.GetCurrentUnixTimeNumber() + reuseTimeInterval * 100;
		}

		/// <summary>
		/// 获取上一次使用的时间
		/// </summary>
		/// <returns>上一次使用的时间</returns>
		public double GetLastUseTime()
		{
			return _lastBorrowTime;
		}

		/// <summary>
		/// 设置上一次使用的时间
		/// </summary>
		/// <param name="lastBorrowTime">上一次使用的时间</param>
		public void SetLastBorrowTime(double lastBorrowTime)
		{
			_lastBorrowTime = lastBorrowTime;
		}

		/// <summary>
		/// 计算通过代理完成一次下载操作消耗的时间
		/// </summary>
		public void RecordResponse()
		{
			ResponseTime = (DateTimeHelper.GetCurrentUnixTimeNumber() - _lastBorrowTime + ResponseTime) / 2;
			_lastBorrowTime = DateTimeHelper.GetCurrentUnixTimeNumber();
		}

		/// <summary>
		/// 记录一次使用此代理下载数据的失败
		/// </summary>
		public void Fail()
		{
			FailedNum++;
		}

		/// <summary>
		/// 取得实际代理信息
		/// </summary>
		/// <returns>实际代理信息</returns>
		public WebProxy GetWebProxy()
		{
			return WebProxy;
		}

		/// <summary>
		/// 设置使用此代理下载数据的失败次数
		/// </summary>
		/// <param name="num">次数</param>
		public void SetFailedNum(int num)
		{
			FailedNum = num;
		}

		/// <summary>
		/// 设置下一次可使用的时间
		/// </summary>
		/// <param name="reuseTimeInterval">代理不被再次使用的间隔</param>
		public void SetReuseTime(int reuseTimeInterval)
		{
			CanReuseTime = DateTimeHelper.GetCurrentUnixTimeNumber() + reuseTimeInterval * 100;
		}
	}
}