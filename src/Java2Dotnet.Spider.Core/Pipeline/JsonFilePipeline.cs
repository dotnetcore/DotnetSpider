using System.IO;
using System.Text;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core.Utils;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.Core.Pipeline
{
	/// <summary>
	/// Store results to files in JSON format.
	/// </summary>
	public class JsonFilePipeline : FilePersistentBase, IPipeline
	{
		/// <summary>
		/// New JsonFilePageModelPipeline with default path "/data/webmagic/"
		/// </summary>
		public JsonFilePipeline()
		{
			SetPath("/data/dotnetspider");
		}

		public JsonFilePipeline(string path)
		{
			SetPath(path);
		}

		public void Process(ResultItems resultItems, ISpider spider)
		{
			string path = BasePath + "/" + spider.Identity + "/";
			try
			{
				FileInfo file = PrepareFile(path + Encrypt.Md5Encrypt(resultItems.Request.Url.ToString()) + ".json");
				using (StreamWriter printWriter = new StreamWriter(file.OpenWrite(), Encoding.UTF8))
				{
					printWriter.WriteLine(JsonConvert.SerializeObject(resultItems.Results));
				}
			}
			catch (IOException e)
			{
				Logger.Warn("write file error", e);
				throw;
			}
		}

		public void Dispose()
		{
		}
	}
}
