using System;
using System.Threading;
using Java2Dotnet.Spider.Redial.AtomicExecutor;
using Java2Dotnet.Spider.Redial.NetworkValidater;
using Java2Dotnet.Spider.Redial.Redialer;
using NLog;

namespace Java2Dotnet.Spider.Redial.RedialManager
{
	public abstract class BaseRedialManager : IRedialManager
	{
		public abstract void WaitforRedialFinish();
		public abstract RedialResult Redial();

		public INetworkValidater NetworkValidater { get; set; } = new DefaultNetworkValidater();

#if !NET_CORE
		public IRedialer Redialer { get; set; } = new H3CSshAdslRedialer();
#else
		public IRedialer Redialer { get; set; } = new AdslRedialer();
#endif

		public abstract IAtomicExecutor AtomicExecutor { get; }

		/// <summary>
		/// Redial interval, default is 5 minitues.
		/// </summary>
		public int RedialInterval { get; set; } = 5;

		/// <summary>
		/// 代码还在测试时大部分在Office里是不能也没有必要拨号（企业网）
		/// 可以测试这个值以跳过拨号以免生产环境中拨号引起调试阻塞
		/// </summary>
		public bool Skip { get; set; } = false;

		public ILogger Logger { get; set; }

		public BaseRedialManager()
		{
			Logger = LogManager.GetCurrentClassLogger();
		}

		protected void RedialInternet()
		{
			if (Redialer == null)
			{
				return;
			}
			Logger.Info("Redialing...");
			Redialer.Redial();

			if (NetworkValidater == null)
			{
				return;
			}

			while (!NetworkValidater.Wait())
			{
				Redialer.Redial();
			}
			Thread.Sleep(2000);
		}
	}
}
