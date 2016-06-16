using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Configuration;
using Newtonsoft.Json.Linq;
using Java2Dotnet.Spider.Extension.Model;
using System;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.ORM;
using System.Reflection;
using System.Linq;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.Extension
{
	public class SpiderContext
	{
		private HashSet<Type> Types { get; set; }

		// build it internal
		public List<Entity> Entities { get; internal set; } = new List<Entity>();

		public string SpiderName { get; set; }
		public string UserId { get; set; } = "DOTNETSPIDER";
		public string TaskGroup { get; set; } = "DOTNETSPIDER";
		public int ThreadNum { get; set; } = 1;
		public int Deep { get; set; } = int.MaxValue;
		public int EmptySleepTime { get; set; } = 15000;
		public int CachedSize { get; set; } = 1;
		public Configuration.Scheduler Scheduler { get; set; }
		public Configuration.Downloader Downloader { get; set; }
		public Site Site { get; set; }
		public bool SkipWhenResultIsEmpty { get; set; } = false;
		public Redialer Redialer { get; set; }
		public List<PrepareStartUrls> PrepareStartUrls { get; set; } = new List<PrepareStartUrls>();
		public Dictionary<string, Dictionary<string, object>> StartUrls { get; set; } = new Dictionary<string, Dictionary<string, object>>();
		public List<Configuration.Pipeline> Pipelines { get; set; } = new List<Configuration.Pipeline>();
		public List<PageHandler> PageHandlers { get; set; } = new List<PageHandler>();
		public TargetUrlsHandler TargetUrlsHandler { get; set; }
		public List<Configuration.TargetUrlExtractor> TargetUrlExtractInfos { get; set; } = new List<Configuration.TargetUrlExtractor>();
		public List<EnviromentValue> EnviromentValues { get; set; } = new List<EnviromentValue>();
		public Validations Validations { get; set; }
		public CookieTrapper GetCookie { get; set; }

		internal bool IsBuilt { get; set; }

		public SpiderContext SetSpiderName(string spiderName)
		{
			SpiderName = spiderName;
			return this;
		}

		public SpiderContext SetUserId(string userId)
		{
			UserId = userId;
			return this;
		}

		public SpiderContext SetTaskGroup(string taskGroup)
		{
			TaskGroup = taskGroup;
			return this;
		}

		public SpiderContext SetThreadNum(int threadNum)
		{
			ThreadNum = threadNum;
			return this;
		}

		public SpiderContext SetDeep(int deep)
		{
			Deep = deep;
			return this;
		}

		public SpiderContext SetEmptySleepTime(int emptySleepTime)
		{
			EmptySleepTime = emptySleepTime;
			return this;
		}

		public SpiderContext SetCachedSize(int cachedSize)
		{
			CachedSize = cachedSize;
			return this;
		}

		public SpiderContext SetScheduler(Configuration.Scheduler scheduler)
		{
			Scheduler = scheduler;
			return this;
		}

		public SpiderContext SetDownloader(Configuration.Downloader downloader)
		{
			Downloader = downloader;
			return this;
		}

		public SpiderContext SetSite(Site site)
		{
			Site = site;
			return this;
		}

		public SpiderContext AddPrepareStartUrls(PrepareStartUrls prepareStartUrls)
		{
			PrepareStartUrls.Add(prepareStartUrls);
			return this;
		}

		public SpiderContext AddPipeline(Configuration.Pipeline pipeline)
		{
			Pipelines.Add(pipeline);
			return this;
		}

		public SpiderContext AddEntityType(Type type)
		{
			if (typeof(ISpiderEntity).IsAssignableFrom(type))
			{
#if NET_CORE
				Entities.Add(ConvertToEntity(type.GetTypeInfo()));
#else
				Entities.Add(ConvertToEntity(type));
#endif
				EnviromentValues = type.GetTypeInfo().GetCustomAttributes<EnviromentExtractBy>().Select(e => new EnviromentValue
				{
					Name = e.Name,
					Selector = new Selector
					{
						Expression = e.Expression,
						Type = e.Type
					}
				}).ToList();
			}
			else
			{
				throw new SpiderExceptoin($"Type: {type.FullName} is not a ISpiderEntity.");
			}

			return this;
		}

		public SpiderContext AddTargetUrlExtractor(Configuration.TargetUrlExtractor targetUrlExtractor)
		{
			TargetUrlExtractInfos.Add(targetUrlExtractor);
			return this;
		}

		public SpiderContext AddStartUrl(string url)
		{
			StartUrls.Add(url, null);
			return this;
		}

		public SpiderContext AddStartUrl(string url, Dictionary<string, object> extras)
		{
			StartUrls.Add(url, extras);
			return this;
		}

		public SpiderContext AddStartUrls(IEnumerable<string> urls)
		{
			foreach (var url in urls)
			{
				StartUrls.Add(url, null);
			}
			return this;
		}

		public SpiderContext AddStartUrls(Dictionary<string, Dictionary<string, object>> urls)
		{
			foreach (var pair in urls)
			{
				StartUrls.Add(pair.Key, pair.Value);
			}
			return this;
		}

		public ISpider ToDefaultSpider()
		{
			return new DefaultSpider("", new Site());
		}

		internal void Build()
		{
			if (!IsBuilt)
			{
				if (Site == null)
				{
					Site = new Site();
				}

				foreach (var url in StartUrls)
				{
					Site.AddStartUrl(url.Key, url.Value);
				}

				IsBuilt = true;
			}
		}

		private static string GetEntityName(Type type)
		{
			return type.FullName;
		}

#if !NET_CORE

		private static Entity ConvertToEntity(Type entityType)
		{
			Entity entity = new Entity();
			entity.Name = GetEntityName(entityType);
			TypeExtractBy extractByAttribute = entityType.GetCustomAttribute<TypeExtractBy>();
			if (extractByAttribute != null)
			{
				entity.Selector = new Selector { Expression = extractByAttribute.Expression, Type = extractByAttribute.Type };
				entity.Multi = extractByAttribute.Multi;
			}
			entity.Schema = entityType.GetCustomAttribute<Schema>();
			var indexes = entityType.GetCustomAttribute<Indexes>();
			if (indexes != null)
			{
				entity.Indexes = indexes.Index?.Select(i => i.Split(',')).ToList();
				entity.Uniques = indexes.Unique?.Select(i => i.Split(',')).ToList();
				entity.Primary = indexes.Primary?.Split(',');

				entity.AutoIncrement = indexes.AutoIncrement;
			}

			var updates = entityType.GetCustomAttribute<Update>();
			if (updates != null)
			{
				entity.Updates = updates.Columns;
			}

			var properties = entityType.GetProperties();
			foreach (var propertyInfo in properties)
			{
				FieldMetadata field = new FieldMetadata();
				var storeAs = propertyInfo.GetCustomAttribute<StoredAs>();
				var extractBy = propertyInfo.GetCustomAttribute<PropertyExtractBy>();

				if (extractBy != null)
				{
					field.Multi = extractBy.Multi;
					field.Option = extractBy.Option;
					field.Selector = new Selector() { Expression = extractBy.Expression, Type = extractBy.Type, Argument = extractBy.Argument };
				}

				if (storeAs != null)
				{
					field.Name = storeAs.Name;
					field.DataType = ConvertDataTypeToString(storeAs);
				}
				else
				{
					field.Name = propertyInfo.Name;
				}

				foreach (var formatter in propertyInfo.GetCustomAttributes<Formatter>(true))
				{
					field.Formatters.Add((JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(formatter)));
				}

				entity.EntityMetadata.Fields.Add(field);
			}

			entity.Stopping = entityType.GetCustomAttribute<Stopping>();

			return entity;
		}
#else
		private static Entity ConvertToEntity(TypeInfo entityType)
		{
			Entity json = new Entity();
			json.Identity = GetEntityName(entityType.AsType());
			TypeExtractBy extractByAttribute = entityType.GetCustomAttribute<TypeExtractBy>();
			if (extractByAttribute != null)
			{
				json.Selector = new Selector { Expression = extractByAttribute.Expression, Type = extractByAttribute.Type };
				json.Multi = extractByAttribute.Multi;
			}
			json.Schema = entityType.GetCustomAttribute<Schema>();
			var indexes = entityType.GetCustomAttribute<Indexes>();
			if (indexes != null)
			{
				json.Indexes = indexes.Index?.Select(i => i.Split(',')).ToList();
				json.Uniques = indexes.Unique?.Select(i => i.Split(',')).ToList();
				json.Primary = indexes.Primary?.Split(',');

				json.AutoIncrement = indexes.AutoIncrement;
			}

			var updates = entityType.GetCustomAttribute<Update>();
			if (updates != null)
			{
				json.Updates = updates.Columns;
			}
			var properties = entityType.AsType().GetProperties();
			foreach (var propertyInfo in properties)
			{
				Field field = new Field();
				var storeAs = propertyInfo.GetCustomAttribute<StoredAs>();
				var extractBy = propertyInfo.GetCustomAttribute<PropertyExtractBy>();

				if (extractBy != null)
				{
					field.Selector = new Selector() { Expression = extractBy.Expression, Type = extractBy.Type };
				}

				if (storeAs != null)
				{
					field.Name = storeAs.Name;
					field.DataType = ConvertDataTypeToString(storeAs);
				}
				else
				{
					field.Name = propertyInfo.Name;
				}

				foreach (var formatter in propertyInfo.GetCustomAttributes<Formatter>(true))
				{
					field.Formatters.Add((JObject)JsonConvert.SerializeObject(formatter));
				}

				json.Fields.Add(field);
			}

			json.Stopping = entityType.GetCustomAttribute<Stopping>();

			return json;
		}
#endif

		protected static string ConvertDataTypeToString(StoredAs storedAs)
		{
			string reslut = "";

			switch (storedAs.Type)
			{
				case DataType.Bool:
					{
						reslut = "bool";
						break;
					}
				case DataType.Date:
					{
						reslut = "date";
						break;
					}
				case DataType.Time:
					{
						reslut = "time";
						break;
					}
				case DataType.Text:
					{
						reslut = "text";
						break;
					}

				case DataType.String:
					{
						reslut = $"{storedAs.Type.ToString().ToLower()}({storedAs.Lenth})";
						break;
					}
			}

			return reslut;
		}
	}

	public class LinkSpiderContext : SpiderContext
	{
		public Dictionary<string, SpiderContext> NextSpiderContexts { get; set; }
	}
}
