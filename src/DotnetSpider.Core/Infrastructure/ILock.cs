namespace DotnetSpider.Core.Infrastructure
{
	public interface ILocker
	{
		void Lock();
		void Release();
	}
}
