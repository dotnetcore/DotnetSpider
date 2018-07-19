namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 锁的接口
	/// </summary>
	public interface ILocker
	{
		/// <summary>
		/// 占用锁
		/// </summary>
		void Lock();

		/// <summary>
		/// 释放锁
		/// </summary>
		void Release();
	}
}
