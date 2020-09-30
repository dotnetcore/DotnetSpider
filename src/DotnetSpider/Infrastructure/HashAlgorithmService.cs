using System.Security.Cryptography;
using System.Threading;

namespace DotnetSpider.Infrastructure
{
	public abstract class HashAlgorithmService : IHashAlgorithmService
	{
		private SpinLock _spin;

		protected abstract HashAlgorithm GetHashAlgorithm();

		public byte[] ComputeHash(byte[] bytes)
		{
			var locker = false;
			try
			{
				//申请获取锁
				_spin.Enter(ref locker);
				return GetHashAlgorithm().ComputeHash(bytes);
			}
			finally
			{
				//工作完毕，或者发生异常时，检测一下当前线程是否占有锁，如果咱有了锁释放它
				//以避免出现死锁的情况
				if (locker)
				{
					_spin.Exit();
				}
			}
		}
	}
}
