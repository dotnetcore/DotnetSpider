using System;
using System.Collections;
using System.IO;
using System.Text;
using DotnetSpider.Core.Infrastructure;
#if NET_CORE
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
			SetPath("data");
		}

		public FilePipeline(string path)
		{
			SetPath(path);
		}

		public string GetDataForlder()
		{
			return $"{BasePath}{Infrastructure.Environment.PathSeperator}{Spider.Identity}{Infrastructure.Environment.PathSeperator}";
		}

		public override void Process(params ResultItems[] resultItems)
		{
			try
			{
				foreach (var resultItem in resultItems)
				{
					string filePath = $"{BasePath}{Infrastructure.Environment.PathSeperator}{Spider.Identity}{Infrastructure.Environment.PathSeperator}{Guid.NewGuid().ToString("N")}.dsd";
					FileInfo file = PrepareFile(filePath);

					using (StreamWriter printWriter = new StreamWriter(file.OpenWrite(), Encoding.UTF8))
					{
						printWriter.WriteLine("url:\t" + resultItem.Request.Url);

						foreach (var entry in resultItem.Results)
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
			}
			catch (Exception e)
			{
				Spider.Log("Write file error.", LogLevel.Error, e);
				throw;
			}
		}
	}
}