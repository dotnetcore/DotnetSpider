using DotnetSpider.Core.Infrastructure;
using NLog;
using System.Reflection;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public interface IAppBase : INamed, IRunable, IIdentity, ITask
	{
	}
    /// <summary>
    /// 标准任务的抽象
    /// </summary>
    public abstract class AppBase : Named, IAppBase
	{

        protected static readonly ILogger Logger = LogCenter.GetLogger();

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
        public IExecuteRecord ExecuteRecord { get; private set; }

        /// <summary>
        /// 任务的实现
        /// </summary>
        protected abstract void Execute(params string[] arguments);

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


        /// <summary>
        /// 异步运行程序
        /// </summary>
        /// <param name="arguments">程序运行参数</param>
        /// <returns></returns>
        public Task RunAsync(params string[] arguments)
        {
            return Task.Factory.StartNew(() => Run(arguments));
        }

        public void Run(params string[] arguments)
        {
            if (ExecuteRecord == null && !string.IsNullOrWhiteSpace(Env.HubServiceUrl))
            {
                ExecuteRecord = new HttpExecuteRecord();
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
            ExecuteRecord?.Remove(TaskId,Name, Identity);
        }
         
    }
}
