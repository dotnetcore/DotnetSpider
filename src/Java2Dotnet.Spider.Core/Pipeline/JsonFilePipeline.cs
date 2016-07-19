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
		public JsonFilePipeline()
		{
			SetPath("/data/files");
		}

		public JsonFilePipeline(string path)
		{
			SetPath(path);
		}

		public void Process(ResultItems resultItems, ISpider spider)
		{
			string path = $"{BasePath}{PathSeperator}{ spider.Identity}{PathSeperator}{Encrypt.Md5Encrypt(resultItems.Request.Url.ToString())}.json";  
			try
			{
				FileInfo file = PrepareFile(path);
				using (StreamWriter printWriter = new StreamWriter(file.OpenWrite(), Encoding.UTF8))
				{
					printWriter.WriteLine(JsonConvert.SerializeObject(resultItems.Results));
				}
			}
			catch (IOException e)
			{
				spider.Logger.Warn("write file error", e);
				throw;
			}
		}

		public void Dispose()
		{
		}
	}
}
