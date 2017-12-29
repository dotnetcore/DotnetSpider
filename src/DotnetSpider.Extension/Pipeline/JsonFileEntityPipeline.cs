using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using DotnetSpider.Core;
using System.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public class JsonFileEntityPipeline : BaseEntityPipeline
	{
		private readonly Dictionary<string, StreamWriter> _writers = new Dictionary<string, StreamWriter>();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override int Process(string entityName, IEnumerable<dynamic> datas, ISpider spider)
		{
			StreamWriter writer;
			var dataFolder = Path.Combine(Env.BaseDirectory, "json", spider.Identity);
			var jsonFile = Path.Combine(dataFolder, $"{entityName}.json");
			if (_writers.ContainsKey(jsonFile))
			{
				writer = _writers[jsonFile];
			}
			else
			{
				if (!Directory.Exists(dataFolder))
				{
					Directory.CreateDirectory(dataFolder);
				}
				writer = new StreamWriter(File.OpenWrite(jsonFile), Encoding.UTF8);
				_writers.Add(jsonFile, writer);
			}

			foreach (var entry in datas)
			{
				writer.WriteLine(entry.ToString());
			}
			return datas.Count();
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach(var writer in _writers)
			{
				writer.Value.Dispose();
			}
		}
	}
}
