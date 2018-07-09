using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using MongoDB.Bson;
using MongoDB.Driver;
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
		/// <param name="model">数据模型</param>
		/// <param name="datas">数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected override int Process(IModel model, IEnumerable<dynamic> datas, ISpider spider)
		{
			if (datas == null || datas.Count() == 0)
			{
				return 0;
			}

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