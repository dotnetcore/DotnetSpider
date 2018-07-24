using DotnetSpider.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

		/// <summary>
		/// 存储数据结果到文件中
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		public override void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
		{
			try
			{
				foreach (var resultItem in resultItems)
				{
					string filePath = Path.Combine(GetDataFolder(sender), $"{ Guid.NewGuid():N}.dsd");
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
								resultItem.Request.AddCountOfResults(list.Count);
								resultItem.Request.AddEffectedRows(list.Count);

							}
							else
							{
								printWriter.WriteLine(entry.Key + ":\t" + entry.Value);

								resultItem.Request.AddCountOfResults(1);
								resultItem.Request.AddEffectedRows(1);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				logger.Error($"Storage data to file failed: {e}.");
				throw;
			}
		}
	}
}