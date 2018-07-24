using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Concurrent;
using DotnetSpider.Common;
using System;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 数据序列化成JSON并存储到文件中
	/// </summary>
	public class JsonFilePipeline : BaseFilePipeline
	{
		private readonly ConcurrentDictionary<string, StreamWriter> _writers = new ConcurrentDictionary<string, StreamWriter>();

		/// <summary>
		/// 构造方法
		/// </summary>
		public JsonFilePipeline() : base("json")
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="interval">数据根目录与程序运行目录路径的相对值</param>
		public JsonFilePipeline(string interval) : base(interval)
		{
		}

		/// <summary>
		/// 数据序列化成JSON并存储到文件中
		/// </summary>
		/// <param name="resultItems">数据结果</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		public override void Process(IList<ResultItems> resultItems, ILogger logger, dynamic sender = null)
		{
			var jsonFile = Path.Combine(GetDataFolder(sender), $"{GetIdentity(sender)}.json");
			try
			{
				var streamWriter = GetStreamWriter(jsonFile);
				foreach (var resultItem in resultItems)
				{
					resultItem.Request.AddCountOfResults(1);
					resultItem.Request.AddEffectedRows(1);

					streamWriter.WriteLine(JsonConvert.SerializeObject(resultItem.Results));
				}
			}
			catch (Exception e)
			{
				logger.Error($"Storage data to file {jsonFile} failed: {e}.");
				throw;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
			foreach (var pair in _writers)
			{
				pair.Value.Dispose();
			}
			_writers.Clear();
		}

		private StreamWriter GetStreamWriter(string file)
		{
			if (_writers.ContainsKey(file))
			{
				return _writers[file];
			}
			else
			{
				var streamWriter = new StreamWriter(File.OpenWrite(file), Encoding.UTF8);
				_writers.TryAdd(file, streamWriter);
				return streamWriter;
			}
		}
	}
}
