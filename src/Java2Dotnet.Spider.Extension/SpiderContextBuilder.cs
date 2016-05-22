using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Java2Dotnet.Spider.Extension.ORM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension
{
	public class SpiderContextBuilder
	{
		public SpiderContext Context { get; }

		protected HashSet<Type> EntiTypes { get; } = new HashSet<Type>();

		public SpiderContextBuilder(SpiderContext context, List<Type> entiTypes)
		{
			if (context == null)
			{
				throw new SpiderExceptoin("SpiderContext is null.");
			}

			if (entiTypes == null || entiTypes.Count == 0)
			{
				throw new SpiderExceptoin("EntiTypes is null.");
			}
			Context = context;

			if (context.Site == null)
			{
				context.Site = new Site();
			}

			Build(entiTypes);
		}

		public SpiderContextBuilder(SpiderContext context, params Type[] entiTypes) : this(context, entiTypes.ToList())
		{
		}

		protected void Build(List<Type> entiTypes)
		{
			foreach (var entiType in entiTypes)
			{
				if (typeof(ISpiderEntity).IsAssignableFrom(entiType))
				{
					EntiTypes.Add(entiType);
				}
				else
				{
					throw new SpiderExceptoin($"Type: {entiType.FullName} is not a ISpiderEntity.");
				}
			}

			foreach (var entiType in EntiTypes)
			{
				Context.Entities.Add(ConvertToEntity(entiType));
				Context.EnviromentValues = entiType.GetTypeInfo().GetCustomAttributes<EnviromentExtractBy>().Select(e => new EnviromentValue
				{
					Name = e.Name,
					Selector = new Selector
					{
						Expression = e.Expression,
						Type = e.Type
					}
				}).ToList();
			}

			foreach (var url in Context.StartUrls)
			{
				Context.Site.AddStartUrl(url.Key, url.Value);
			}
		}

		public static string GetEntityName(Type type)
		{
			return type.FullName;
		}

#if !NET_CORE

		public static Entity ConvertToEntity(Type entityType)
		{
			Entity entity = new Entity();
			entity.Identity = GetEntityName(entityType);
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
				Field field = new Field();
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

				entity.Fields.Add(field);
			}

			entity.Stopping = entityType.GetCustomAttribute<Stopping>();

			return entity;
		}
#else
		public static Entity ConvertToEntity(TypeInfo entityType)
		{
			EntityType json = new EntityType();
			json.Identity = GetEntityName(entityType.AsType());
			json.TargetUrls = entityType.GetCustomAttributes<TargetUrl>().ToList();
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
}
