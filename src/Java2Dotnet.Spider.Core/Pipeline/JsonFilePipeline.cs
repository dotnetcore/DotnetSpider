using System.IO;
using System.Text;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Core.Utils;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace Java2Dotnet.Spider.Core.Pipeline
{
	/// <summary>
	/// Store results to files in JSON format.
	/// </summary>
	public class JsonFilePipeline : BasePipeline
	{
		private string intervalPath = "";

		public JsonFilePipeline()
		{
#if NET_CORE
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				intervalPath = "\\data\\json";
			}
			else
			{
				intervalPath = "/data/json";
			}
#else
			intervalPath="\\data\\json";
#endif
		}

		public JsonFilePipeline(string path)
		{
			intervalPath = path;
		}

		public override void InitPipeline(ISpider spider)
		{
			base.InitPipeline(spider);

			string path;
			if (string.IsNullOrEmpty(intervalPath))
			{
#if NET_CORE
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					path = $"\\{spider.Identity}\\data\\json";
				}
				else
				{
					path = $"/{spider.Identity}/data/json";
				}
#else
				path="\\{spider.Identity}\\data\\json";
#endif
			}
			else
			{
				path = intervalPath;
			}
			SetPath(path);
		}

		public override void Process(ResultItems resultItems)
		{

			try
			{
				string path = $"{BasePath}{PathSeperator}{Spider.Identity}{PathSeperator}{Encrypt.Md5Encrypt(resultItems.Request.Url.ToString())}.json";
				FileInfo file = PrepareFile(path);
				using (StreamWriter printWriter = new StreamWriter(file.OpenWrite(), Encoding.UTF8))
				{
					printWriter.WriteLine(JsonConvert.SerializeObject(resultItems.Results));
				}
			}
			catch (IOException e)
			{
				Spider.Logger.Warn("Write data to json file failed.", e);
				throw;
			}
		}
	}
}
