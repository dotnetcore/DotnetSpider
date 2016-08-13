using System;
using System.Collections;
using System.IO;
using System.Text;
using DotnetSpider.Core.Common;
using NLog;

#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// Store results in files.
	/// </summary>
	public sealed class FilePipeline : BasePipeline
	{
		/// <summary>
		/// create a FilePipeline with default path"/data/dotnetspider/"
		/// </summary>
		public FilePipeline()
		{
#if NET_CORE
			SetPath(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "data\\files" : "data/files");
#else
			SetPath("data\\files");
#endif
		}

		public FilePipeline(string path)
		{
			SetPath(path);
		}

		public string GetDataForlder()
		{
			return $"{BasePath}{PathSeperator}{Spider.Identity}{PathSeperator}";
		}

		public override void Process(ResultItems resultItems)
		{
			try
			{
				string filePath = $"{BasePath}{PathSeperator}{Spider.Identity}{PathSeperator}{Encrypt.Md5Encrypt(resultItems.Request.Url.ToString())}.fd";
				FileInfo file = PrepareFile(filePath);
				using (StreamWriter printWriter = new StreamWriter(file.OpenWrite(), Encoding.UTF8))
				{
					printWriter.WriteLine("url:\t" + resultItems.Request.Url);

					foreach (var entry in resultItems.Results)
					{
						var value = entry.Value as IList;
						if (value != null)
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
			catch (Exception e)
			{
				Logger.Log(LogInfo.Create("Write file error.", Logger.Name, Spider, LogLevel.Warn, e));
				throw;
			}
		}
	}
}