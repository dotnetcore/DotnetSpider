using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Downloader;
using System.Linq;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到MongoDb中
	/// </summary>
	public class MongoDbEntityPipeline : EntityPipeline
	{
		private readonly MongoClient _client;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">连接字符串</param>
		public MongoDbEntityPipeline(string connectString)
		{
			_client = new MongoClient(connectString);
		}

		/// <summary>
		/// 把解析到的爬虫实体数据存到MongoDb中
		/// </summary>
		/// <param name="items">数据</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected override int Process(List<IBaseEntity> items, dynamic sender = null)
		{
			if (items == null) return 0;
			var tableInfo = new TableInfo(items.First().GetType());
			var db = _client.GetDatabase(tableInfo.Schema.Database);
			var collection = db.GetCollection<BsonDocument>(tableInfo.Schema.FullName);

			var action = new Action(() =>
			{
				var result = new List<BsonDocument>();
				foreach (var data in items)
				{
					var item = BsonDocument.Create(data);
					result.Add(item);
				}

				result.Add(BsonDocument.Create(DateTime.Now));
				collection.InsertMany(result);
			});
			if (DatabaseExtensions.UseNetworkCenter)
			{
				NetworkCenter.Current.Execute("db", action);
			}
			else
			{
				action();
			}

			return items.Count;
		}
	}
}