using System.IO;
using System.Text;
using DotnetSpider.Core.Infrastructure;
using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// Store results to files in JSON format.
	/// </summary>
	public class JsonFilePipeline : BaseFilePipeline
	{
		private string _jsonFile;
		private StreamWriter _streamWriter;

		public JsonFilePipeline() : base("json")
		{
		}

		public JsonFilePipeline(string interval) : base(interval)
		{
		}

		public override void Init(ISpider spider)
		{
			base.Init(spider);

			_jsonFile = Path.Combine(DataFolder, $"{spider.Identity}.json");
			_streamWriter = new StreamWriter(File.OpenWrite(_jsonFile), Encoding.UTF8);
		}

		public override void Process(IEnumerable<ResultItems> resultItems)
		{
			try
			{
				foreach (var resultItem in resultItems)
				{
					_streamWriter.WriteLine(JsonConvert.SerializeObject(resultItem.Results));
				}
			}
			catch (IOException e)
			{
				Logger.AllLog(Spider.Identity, "Write data to json file failed.", LogLevel.Error, e);
				throw;
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			_streamWriter.Dispose();
		}
	}
}
