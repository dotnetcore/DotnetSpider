using DotnetSpider.Redial.AtomicExecutor;
using DotnetSpider.Redial.NetworkValidater;
using DotnetSpider.Redial.Redialer;

namespace DotnetSpider.Redial.RedialManager
{
	public interface IRedialManager : IWaitforRedial
	{
		RedialResult Redial();
		INetworkValidater NetworkValidater { get; set; }
		IRedialer Redialer { get; set; }
		IAtomicExecutor AtomicExecutor { get; }
	}

	public interface IWaitforRedial
	{
		void WaitforRedialFinish();
	}
}
