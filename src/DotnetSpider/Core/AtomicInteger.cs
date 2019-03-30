using System.Threading;

namespace DotnetSpider.Core
{
    /// <summary>
    /// 线程安全的计数器
    /// </summary>
    public class AtomicInteger
    {
        private int _value;

        /// <summary>
        /// 获取当前值
        /// </summary>
        public int Value => _value;

        /// <summary>
        /// 构造方法, 起始值为 0
        /// </summary>
        public AtomicInteger()
            : this(0)
        {
        }

        /// <summary>
        /// 使用指定值作为初始值创建新实例
        /// </summary>
        /// <param name="initValue">计算开始值</param>
        public AtomicInteger(int initValue)
        {
            _value = initValue;
        }

        /// <summary>
        /// 递增并返回最新值
        /// </summary>
        /// <returns>递增后的值</returns>
        public int Inc()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// 递减并返回最新值
        /// </summary>
        /// <returns>递减后的值</returns>
        public int Dec()
        {
            return Interlocked.Decrement(ref _value);
        }

        /// <summary>
        /// 比较并设置新值
        /// </summary>
        /// <param name="expectedValue">期望的值</param>
        /// <param name="newValue">新值</param>
        /// <returns>更新成功时返回true</returns>
        public bool CompareAndSet(int expectedValue, int newValue)
        {
            var original = Interlocked.CompareExchange(ref _value, newValue, expectedValue);
            return original == expectedValue;
        }

        /// <summary>
        /// 强制更新为新值
        /// </summary>
        /// <param name="newValue">新的值</param>
        public void Set(int newValue)
        {
            Interlocked.Exchange(ref _value, newValue);
        }
        
        public void Add(int value)
        {
            Interlocked.Add(ref _value, value);
        }
    }
}