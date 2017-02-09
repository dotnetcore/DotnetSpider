using DotnetSpider.Core;

namespace DotnetSpider.Redial
{
	public interface IRedialExecutor: INetworkExecutor
	{
		RedialResult Redial();
		void WaitAll();
		string CreateActionIdentity(string name);
		void DeleteActionIdentity(string identity);
	}
}
