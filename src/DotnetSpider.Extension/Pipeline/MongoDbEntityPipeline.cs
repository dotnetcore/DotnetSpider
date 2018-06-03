using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System;
using DotnetSpider.Core.Redial;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到MongoDb中
	/// </summary>
	public class MongoDbEntityPipeline : ModelPipeline
	{
		private readonly ConcurrentDictionary<string, IMongoCollection<BsonDocument>> _collections = new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();

		private readonly string _connectString;
		private readonly MongoClient _client;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">连接字符串</param>
		public MongoDbEntityPipeline(string connectString)
		{
			_connectString = connectString;

			_client = new MongoClient(_connectString);
		}

		/// <summary>
		/// 把解析到的爬虫实体数据存到MongoDb中
		/// </summary>
		/// <param name="entityName">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected override int Process(IModel model, IEnumerable<dynamic> datas, ISpider spider)
		{
			var db = _client.GetDatabase(model.TableInfo.Database);
			var collection = db.GetCollection<BsonDocument>(model.TableInfo.FullName);

			var action = new Action(() =>
			{
				List<BsonDocument> reslut = new List<BsonDocument>();
				foreach (var data in datas)
				{
					BsonDocument item = BsonDocument.Create(data);
					reslut.Add(item);
				}
				reslut.Add(BsonDocument.Create(DateTime.Now));
				collection.InsertMany(reslut);
			});
			if (DbExecutor.UseNetworkCenter)
			{
				NetworkCenter.Current.Execute("db", action);
			}
			else
			{
				action();
			}
			return datas.Count();
		}
	}
}