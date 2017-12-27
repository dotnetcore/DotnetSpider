using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetSpider.Core.Infrastructure;
using NLog;
#if NET_CORE
#endif

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 存储数据结果到文件中
	/// </summary>
	public class FilePipeline : BaseFilePipeline
	{
		/// <summary>
		/// 数据文件夹地址为: {BaseDirecoty}/data/{Identity}
		/// </summary>
		public FilePipeline() : base("file")
		{
		}

		/// <summary>
		/// 数据文件夹地址为: {BaseDirecoty}/data/{interval}
		/// </summary>
		public FilePipeline(string interval) : base(interval)
		{
		}

		public override void Process(IEnumerable<ResultItems> resultItems)
		{
			try
			{
				foreach (var resultItem in resultItems)
				{
					string filePath = Path.Combine(DataFolder, $"{ Guid.NewGuid():N}.dsd");
					using (StreamWriter printWriter = new StreamWriter(File.OpenWrite(filePath), Encoding.UTF8))
					{
						printWriter.WriteLine("url:\t" + resultItem.Request.Url);

						foreach (var entry in resultItem.Results)
						{
							if (entry.Value is IList value)
							{
								IList list = value;
								printWriter.WriteLine(entry.Key + ":");
								foreach (var o in list)
								{
									printWriter.WriteLine(o);
								}
							}
							else
							{
								printWriter.WriteLine(entry.Key + ":\t" + entry.Value);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.AllLog(Spider.Identity, "Write file error.", LogLevel.Error, e);
				throw;
			}
		}
	}
}