using System.Collections.Generic;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 数据管道抽象, 通过数据管道把解析的数据存到不同的存储中(文件、数据库）
	/// </summary>
	public abstract class BasePipeline : IPipeline
	{
		/// <summary>
		/// 处理页面解析器解析到的数据结果
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="spider">爬虫</param>
		public abstract void Process(IEnumerable<ResultItems> resultItems, ISpider spider);

		/// <summary>
		/// 在使用数据管道前, 进行一些初始化工作, 不是所有的数据管道都需要进行初始化
		/// </summary>
		public virtual void Init() { }

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
		}
	}
}
