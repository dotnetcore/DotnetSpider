using Java2Dotnet.Spider.Log;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using Java2Dotnet.Spider.Redial.NetworkValidater;
using Java2Dotnet.Spider.Redial.Redialer;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	public interface IRedialManager : IWaitforRedial
	{
		RedialResult Redial();
		INetworkValidater NetworkValidater { get; set; }
		IRedialer Redialer { get; set; }
		IAtomicExecutor AtomicExecutor { get; }
		ILogService Logger { get; set; }
	}

	public interface IWaitforRedial
	{
		void WaitforRedialFinish();
	}
}
