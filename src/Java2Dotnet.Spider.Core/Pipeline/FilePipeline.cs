using System;
using System.Collections;
using System.IO;
using System.Text;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core.Utils;

namespace Java2Dotnet.Spider.Core.Pipeline
{
	/// <summary>
	/// Store results in files.
	/// </summary>
	public sealed class FilePipeline : FilePersistentBase, IPipeline
	{
		/// <summary>
		/// create a FilePipeline with default path"/data/dotnetspider/"
		/// </summary>
		public FilePipeline()
		{
			SetPath(@"\data\files\");
		}

		public FilePipeline(string path)
		{
			SetPath(path);
		}

		public void Process(ResultItems resultItems, ISpider spider)
		{
			StringBuilder builer = new StringBuilder(BasePath);
			string filePath = $"{BasePath}{PathSeperator}{spider.Identity}{PathSeperator}{Encrypt.Md5Encrypt(resultItems.Request.Url.ToString())}.fd";
			try
			{
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
				Logger.Warn("Write file error.", e);
				throw;
			}
		}

		public void Dispose()
		{
		}
	}
}