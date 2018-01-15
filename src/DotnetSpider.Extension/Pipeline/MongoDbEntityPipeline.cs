using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到MongoDb中
	/// </summary>
	public class MongoDbEntityPipeline : BaseEntityPipeline
	{
		private readonly ConcurrentDictionary<string, IMongoCollection<BsonDocument>> _collections = new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();

		private readonly string _connectString;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">连接字符串</param>
		public MongoDbEntityPipeline(string connectString)
		{
			_connectString = connectString;
		}

		/// <summary>
		/// 添加爬虫实体类的定义
		/// </summary>
		/// <param name="entityDefine">爬虫实体类的定义</param>
		public override void AddEntity(IEntityDefine entityDefine)
		{
			if (entityDefine.TableInfo == null)
			{
				Logger.Log($"Schema is necessary, skip {GetType().Name} for {entityDefine.Name}.", Level.Warn);
				return;
			}

			MongoClient client = new MongoClient(_connectString);
			var db = client.GetDatabase(entityDefine.TableInfo.Database);

			_collections.TryAdd(entityDefine.Name, db.GetCollection<BsonDocument>(entityDefine.TableInfo.CalculateTableName()));
		}

		/// <summary>
		/// 把解析到的爬虫实体数据存到MongoDb中
		/// </summary>
		/// <param name="entityName">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		public override int Process(string entityName, IEnumerable<dynamic> datas, ISpider spider)
		{
			if (_collections.TryGetValue(entityName, out var collection))
			{
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
			}
			return datas.Count();
		}
	}
}