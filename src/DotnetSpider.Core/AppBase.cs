using DotnetSpider.Core.Infrastructure;
using NLog;
using System.Reflection;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public interface IAppBase : INamed, IRunable, IIdentity, ITask
	{
	}

	public abstract class AppBase : IAppBase
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();

		public virtual string Identity { get; set; }

		public string Name { get; set; }

		public virtual string TaskId { get; set; }

		public IExecuteRecord ExecuteRecord { get; private set; }

		protected abstract void RunApp(params string[] arguments);

		protected AppBase()
		{
			var type = GetType();
			var nameAttribute = type.GetCustomAttribute<TaskName>();
			Name = nameAttribute != null ? nameAttribute.Name : type.Name;
		}

		protected AppBase(string name) : this()
		{
			Name = name;
		}

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			});
		}

		public void Run(params string[] arguments)
		{
			if (ExecuteRecord == null && !string.IsNullOrEmpty(Env.EnterpiseServiceUrl))
			{
				ExecuteRecord = new HttpExecuteRecord();
			}

			if (!AddExecuteRecord())
			{
				Logger.AllLog(Identity, "Can not add execute record.", LogLevel.Error);
			}
			try
			{
				RunApp(arguments);
			}
			finally
			{
				RemoveExecuteRecord();
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

		private void RemoveExecuteRecord()
		{
			ExecuteRecord?.Remove(TaskId);
		}
	}
}
