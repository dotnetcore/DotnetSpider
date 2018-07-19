using DotnetSpider.Common;
using DotnetSpider.Core.Infrastructure;
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
		/// 唯一标识
		/// </summary>
		public virtual ILogger Logger { get; protected set; }

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
					throw new ArgumentException($"{nameof(Identity)} should not be empty or null.");
				}
				if (value.Length > Env.IdentityMaxLength)
				{
					throw new ArgumentException($"Length of identity should less than {Env.IdentityMaxLength}.");
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
		protected DateTime StartTime { get; private set; }

		/// <summary>
		/// end time of spider.
		/// </summary>
		protected DateTime ExitTime { get; private set; } = DateTime.MinValue;

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

		public AppBase()
		{
			LogUtil.Init();
		}

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

			Logger = LogUtil.Create(Identity);

			if (ExecuteRecord == null)
			{
				if (!string.IsNullOrWhiteSpace(Env.HubServiceUrl))
				{
					ExecuteRecord = new HttpExecuteRecord(Logger);
				}
				else
				{
					ExecuteRecord = new LogExecuteRecord(Logger);
				}
			}

			if (!ExecuteRecord.Add(TaskId, Name, Identity))
			{
				Logger.Error($"Add execute record: {Identity} failed.");
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
				Logger.Information($"Consume: {(ExitTime - StartTime).TotalSeconds} seconds.");
				PrintInfo.PrintLine();
			}
		}

		public abstract void Pause(Action action = null);

		public abstract void Contiune();

		public abstract void Exit(Action action = null);
	}
}
