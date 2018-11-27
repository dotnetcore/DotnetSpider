using DotnetSpider.Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 标准任务的抽象
	/// </summary>
	public abstract class AppBase : Named, IAppBase
	{
		private string _identity = Guid.NewGuid().ToString("N");

		/// <summary>
		/// 日志系统
		/// 可以实现此实例，自定义日志
		/// </summary>
		public ILoggerFactory LoggerFactory;


		/// <summary>
		/// 唯一标识
		/// </summary>
		public virtual Microsoft.Extensions.Logging.ILogger Logger { get; set; }

		/// <summary>
		/// 唯一标识
		/// </summary>
		public string Identity
		{
			get => _identity;
			set
			{
				CheckIfRunning();

				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(Identity)} should not be empty or null");
				}

				if (value.Length > Env.IdentityMaxLength)
				{
					throw new ArgumentException($"Length of identity should less than {Env.IdentityMaxLength}");
				}

				_identity = value;
			}
		}

		/// <summary>
		/// 任务编号
		/// </summary>
		public virtual string TaskId { get; set; }

		/// <summary>
		/// start time of spider.
		/// </summary>
		public DateTime StartTime { get; private set; }

		/// <summary>
		/// end time of spider.
		/// </summary>
		public DateTime ExitTime { get; private set; }

		/// <summary>
		/// 运行记录接口
		/// 程序在运行前应该添加相应的运行记录, 任务结束后删除对应的记录, 企业服务依赖运行记录数据显示正在运行的任务
		/// </summary>
		public IExecuteRecord ExecuteRecord { get; set; }

		protected abstract void CheckIfRunning();

		/// <summary>
		/// 任务的实现
		/// </summary>
		protected abstract void Execute(params string[] arguments);

		/// <summary>
		/// 异步运行程序
		/// </summary>
		/// <param name="arguments">程序运行参数</param>
		/// <returns></returns>
		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() => Run(arguments));
		}

		/// <summary>
		/// 运行程序
		/// </summary>
		/// <param name="arguments">程序运行的参数</param>
		public void Run(params string[] arguments)
		{
			PrintInfo.Print();

			if (this.LoggerFactory == null)
			{
				this.LoggerFactory = new LoggerFactory();
				if (Env.SerilogAppSettings)
				{
					Log.Logger = new LoggerConfiguration()
						.ReadFrom.AppSettings()
						.CreateLogger();
					this.LoggerFactory.AddSerilog();
				}
				else
				{
					var loggerConfiguration = new LoggerConfiguration()
					.MinimumLevel.Verbose()
					.WriteTo.Console(theme: SerilogConsoleTheme.ConsoleLogTheme)
					.WriteTo.RollingFile("dotnetspider.log");
					if (Env.HubServiceLog)
					{
						loggerConfiguration.WriteTo.Http(Env.HubServiceLogUrl, Env.HubServiceToken)
							.Enrich.WithProperty("NodeId", Env.NodeId).Enrich.WithProperty("Identity", Identity);
					}
					Log.Logger = loggerConfiguration.CreateLogger();
					this.LoggerFactory.AddSerilog();
				}
			}

			Logger = this.LoggerFactory.CreateLogger("DS");

			if (ExecuteRecord == null)
			{
				ExecuteRecord = string.IsNullOrWhiteSpace(Env.HubServiceUrl)
					? (IExecuteRecord)new NullExecuteRecord()
					: new HttpExecuteRecord();
				ExecuteRecord.Logger = Logger;
			}

			if (!ExecuteRecord.Add(TaskId, Name, Identity))
			{
				Logger.LogError($"Add execute record: {Identity} failed");
			}

			try
			{
				StartTime = DateTime.Now;
				Execute(arguments);
			}
			finally
			{
				ExitTime = DateTime.Now;
				ExecuteRecord.Remove(TaskId, Name, Identity);
				Logger.LogInformation($"Consume: {(ExitTime - StartTime).TotalSeconds} seconds");
				PrintInfo.PrintLine();
			}
		}

		public abstract void Pause(Action action = null);

		public abstract void Continue();

		public abstract void Exit(Action action = null);
	}
}