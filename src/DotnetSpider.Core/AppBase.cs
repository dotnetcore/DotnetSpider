using DotnetSpider.Core.Infrastructure;
using Serilog;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 标准任务接口
	/// </summary>
	public interface IAppBase : IRunable, IIdentity, ITask, INamed
	{
		ILogger Logger { get; }
	}

	/// <summary>
	/// 标准任务的抽象
	/// </summary>
	public abstract class AppBase : Named, IAppBase
	{
		private ILogger _logger;

		/// <summary>
		/// 唯一标识
		/// </summary>
		public virtual ILogger Logger
		{
			get
			{
				if (_logger == null && !string.IsNullOrWhiteSpace(Identity))
				{
					_logger = LogUtil.Create(Identity);
				}
				return _logger;
			}
			set
			{
				if (value != null)
				{
					_logger = value;
				}
			}
		}

		/// <summary>
		/// 唯一标识
		/// </summary>
		public virtual string Identity { get; set; }

		/// <summary>
		/// 任务编号
		/// </summary>
		public virtual string TaskId { get; set; }

		/// <summary>
		/// 运行记录接口
		/// 程序在运行前应该添加相应的运行记录, 任务结束后删除对应的记录, 企业服务依赖运行记录数据显示正在运行的任务
		/// </summary>
		public IExecuteRecord ExecuteRecord { get; set; }

		/// <summary>
		/// 任务的实现
		/// </summary>
		protected abstract void Execute(params string[] arguments);

		/// <summary>
		/// 构造函数
		/// </summary>
		protected AppBase()
		{
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="name">任务名称</param>
		protected AppBase(string name) : base()
		{
			Name = name;
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
			if (ExecuteRecord == null && !string.IsNullOrWhiteSpace(Env.HubServiceUrl))
			{
				ExecuteRecord = new HttpExecuteRecord();
			}

			if (ExecuteRecord != null)
			{
				ExecuteRecord.Logger = Logger;
			}

			if (!AddExecuteRecord())
			{
				Logger.Error($"Can not add execute record: {Identity}.");
			}
			try
			{
				Execute(arguments);
			}
			finally
			{
				ExecuteRecord?.Remove(TaskId, Name, Identity);
			}
		}

		private bool AddExecuteRecord()
		{
			if (ExecuteRecord == null)
			{
				return true;
			}
			else
			{
				return ExecuteRecord.Add(TaskId, Name, Identity);
			}
		}
	}
}
