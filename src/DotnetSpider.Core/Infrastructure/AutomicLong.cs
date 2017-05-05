using System.Threading;

namespace DotnetSpider.Core.Infrastructure
{
	public class AutomicLong
	{
		///
		/// 计数
		///
		private long _value;

		///
		/// 获取当前值
		///
		public long Value => _value;

		///
		/// 使用0作为初始值创建新实例
		///
		public AutomicLong()
			: this(0)
		{
		}

		///
		/// 使用指定值作为初始值创建新实例
		///
		public AutomicLong(long initValue)
		{
			_value = initValue;
		}

		///
		/// 递增并返回最新值
		///
		/// 递增后的值
		public long Inc()
		{
			return Interlocked.Increment(ref _value);
		}

		///
		/// 递减并返回最新值
		///
		/// 递减后的值
		public long Dec()
		{
			return Interlocked.Decrement(ref _value);
		}

		///
		/// 比较并设置新值
		///
		/// 期望的值
		/// 新值
		/// 更新成功时返回true
		public bool CompareAndSet(long expectedValue, long newValue)
		{
			long original = Interlocked.CompareExchange(ref _value, newValue, expectedValue);
			return original == expectedValue;
		}

		///
		/// 强制更新为新值
		///
		/// 新的值
		public void Set(long newValue)
		{
			Interlocked.Exchange(ref _value, newValue);
		}
	}
}
