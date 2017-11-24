using System;

namespace DotnetSpider.Core.Pipeline
{
    /// <summary>
    /// Pipeline is the persistent and offline process part of crawler. 
    /// The interface Pipeline can be implemented to customize ways of persistent
    /// 负责数据的存储, 已实现文件存储, MySql存储, MySqlFile存储(脚本)，MSSQL存储，MongoDb存储
    /// </summary>
    public interface IPipeline : IDisposable
	{
		ISpider Spider { get; }

		void InitPipeline(ISpider spider);

		/// <summary>
		/// Process extracted results.
		/// </summary>
		/// <param name="resultItems"></param>
		void Process(params ResultItems[] resultItems);
	}
}