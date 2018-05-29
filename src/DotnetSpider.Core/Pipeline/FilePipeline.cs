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
	/// Store results in files.
	/// </summary>
	public  class FilePipeline : BasePipeline
	{
		/// <summary>
		/// create a FilePipeline with default path"/data/dotnetspider/"
		/// </summary>
		public FilePipeline()
		{
			SetPath("data");
		}

		public FilePipeline(string path)
		{
			SetPath(path);
		}

		public string GetDataForlder()
		{
			return $"{BasePath}{Env.PathSeperator}{Spider.Identity}{Env.PathSeperator}";
		}

		public override void Process(IList<ResultItems> resultItems,ISpider spider)
		{
			try
			{
				foreach (var resultItem in resultItems)
				{
					string filePath = $"{BasePath}{Env.PathSeperator}{Spider.Identity}{Env.PathSeperator}{Guid.NewGuid():N}.dsd";
					FileInfo file = PrepareFile(filePath);

					using (StreamWriter printWriter = new StreamWriter(file.OpenWrite(), Encoding.UTF8))
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