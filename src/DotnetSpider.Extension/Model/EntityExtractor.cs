using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Infrastructure;
using System;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 爬虫实体的解析器
	/// </summary>
	/// <typeparam name="T">爬虫实体类的类型</typeparam>
	public class EntityExtractor<T> : IEntityExtractor<T>
	{
		/// <summary>
		/// 日志接口
		/// </summary>
		protected static readonly ILogger Logger = DLog.GetLogger();

		/// <summary>
		/// 爬虫实体类的定义
		/// </summary>
		public IEntityDefine EntityDefine { get; }

		/// <summary>
		/// 对解析的结果进一步加工操作
		/// </summary>
		public IDataHandler<T> DataHandler { get; }

		/// <summary>
		/// 解析器的名称, 默认值是爬虫实体类型的全称
		/// </summary>
		public string Name => EntityDefine?.Name;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public EntityExtractor(IDataHandler<T> dataHandler = null, string tableName = null)
		{
			EntityDefine = new EntityDefine<T>();
			if (!string.IsNullOrWhiteSpace(tableName))
			{
				EntityDefine.TableInfo.Name = tableName;
			}
			DataHandler = dataHandler;
		}

		/// <summary>
		/// 解析成爬虫实体对象
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <returns>爬虫实体对象</returns>
		public List<T> Extract(Page page)
		{
			List<T> result = new List<T>();
			if (EntityDefine.SharedValues != null && EntityDefine.SharedValues.Count > 0)
			{
				foreach (var enviromentValue in EntityDefine.SharedValues)
				{
					string name = enviromentValue.Name;
					var value = page.Selectable.Select(enviromentValue.ToSelector()).GetValue();
					page.Request.PutExtra(name, value);
				}
			}
			ISelector selector = EntityDefine.SelectorAttribute.ToSelector();
			if (selector != null && EntityDefine.Multi)
			{
				var list = page.Selectable.SelectList(selector).Nodes();
				if (list == null || list.Count == 0)
				{
					result = null;
				}
				else
				{
					if (EntityDefine.Take > 0)
					{
						list = list.Take(EntityDefine.Take).ToList();
					}

					for (int i = 0; i < list.Count; ++i)
					{
						var item = list[i];
						var obj = ExtractSingle(page, item, i);
						if (obj != null)
						{
							result.Add(obj);
						}
					}
				}
			}
			else
			{
				ISelectable select = selector == null ? page.Selectable : page.Selectable.Select(selector);

				if (select != null)
				{
					var item = ExtractSingle(page, select, 0);
					result = item != null ? new List<T> { item } : null;
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		private T ExtractSingle(Page page, ISelectable item, int index)
		{
			T dataObject = Activator.CreateInstance<T>();

			bool skip = false;
			foreach (var field in EntityDefine.Columns)
			{
				var fieldValue = ExtractField(item, page, field, index);
				if (fieldValue == null)
				{
					if (field.NotNull)
					{
						skip = true;
						break;
					}
				}
				else
				{
					field.Property.SetValue(dataObject, fieldValue);
				}
			}

			return skip ? default(T) : dataObject;

			//if (dataObject != null && EntityDefine.LinkToNexts != null)
			//{
			//	foreach (var targetUrl in EntityDefine.LinkToNexts)
			//	{
			//		Dictionary<string, dynamic> extras = new Dictionary<string, dynamic>();
			//		if (targetUrl.Extras != null)
			//		{
			//			foreach (var extra in targetUrl.Extras)
			//			{
			//				extras.Add(extra, result[extra]);
			//			}
			//		}
			//		Dictionary<string, dynamic> allExtras = new Dictionary<string, dynamic>();
			//		foreach (var extra in page.Request.Extras.Union(extras))
			//		{
			//			allExtras.Add(extra.Key, extra.Value);
			//		}
			//		var value = result[targetUrl.PropertyName];
			//		if (value != null)
			//		{
			//			page.AddTargetRequest(new Request(value.ToString(), allExtras));
			//		}
			//	}
			//}
		}

		private object ExtractField(ISelectable item, Page page, Column field, int index)
		{
			if (field == null)
			{
				return null;
			}
			ISelector selector = field.SelectorAttribute.ToSelector();
			if (selector == null)
			{
				return null;
			}

			if (selector is EnviromentSelector)
			{
				var enviromentSelector = selector as EnviromentSelector;
				var value = SelectorUtil.GetEnviromentValue(enviromentSelector.Field, page, index);
				foreach (var formatter in field.Formatters)
				{
#if DEBUG
					try
					{
#endif
						value = formatter.Formate(value);
#if DEBUG
					}
					catch (Exception e)
					{
						Logger.NLog(e.ToString(), Level.Error);
					}
#endif
				}
				return TryConvert(value, field.DataType);
			}
			else
			{
				bool needCount = field.Option == PropertyDefine.Options.Count;
				if (needCount)
				{
					var values = item.SelectList(selector).Nodes();
					return values.Count;
				}
				else
				{
					var value = (object)item.Select(selector)?.GetValue(field.Option == PropertyDefine.Options.PlainText);

					foreach (var formatter in field.Formatters)
					{
#if DEBUG
						try
						{
#endif
							value = formatter.Formate(value);
#if DEBUG
						}
						catch (Exception e)
						{
							Logger.NLog(e.ToString(), Level.Error);
						}
#endif
					}

					return TryConvert(value, field.DataType);
				}
			}
		}

		private object TryConvert(object value, Type type)
		{
			try
			{
				if (value != null)
				{
					return Convert.ChangeType(value, type);
				}
			}
			catch
			{
			}
			return null;
		}
	}
}
