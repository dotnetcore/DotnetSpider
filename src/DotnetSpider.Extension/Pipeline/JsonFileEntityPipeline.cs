using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using DotnetSpider.Core;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Common;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据序列化成JSON并存到文件中
	/// </summary>
	public class JsonFileEntityPipeline : ModelPipeline
	{
		private readonly Dictionary<string, StreamWriter> _writers = new Dictionary<string, StreamWriter>();

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
			foreach (var writer in _writers)
			{
				writer.Value.Dispose();
			}
		}

		/// <summary>
		/// 把解析到的爬虫实体数据序列化成JSON并存到文件中
		/// </summary>
		/// <param name="model">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		protected override int Process(IModel model, IList<dynamic> datas, ILogger logger, dynamic sender = null)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}

			StreamWriter writer;
			var identity = GetIdentity(sender);
			var dataFolder = Path.Combine(Env.BaseDirectory, "json", identity);
			var jsonFile = Path.Combine(dataFolder, $"{model.Table.FullName}.json");
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
			return datas.Count;
		}
	}
}
