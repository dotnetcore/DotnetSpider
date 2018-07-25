using DotnetSpider.Common;
using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 数据管道抽象, 通过数据管道把解析的数据存到不同的存储中(文件、数据库）
	/// </summary>
	public abstract class BasePipeline : Named, IPipeline
	{
		/// <summary>
		/// 处理页面解析器解析到的数据结果
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		public abstract void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
		}

		protected string GetIdentity(dynamic sender)
		{
			if (sender == null)
			{
				throw new SpiderException("Sender should not be null.");
			}
			try
			{
				return sender.Identity;
			}
			catch
			{
				throw new SpiderException("Sender should be a IIdentity object.");
			}
		}
	}
}