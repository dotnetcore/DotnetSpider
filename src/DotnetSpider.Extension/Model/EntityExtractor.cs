using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using DotnetSpider.Core.Infrastructure;
using Cassandra;

namespace DotnetSpider.Extension.Model
{
	public class EntityExtractor<T> : IEntityExtractor<T>
	{
		public IEntityDefine EntityDefine { get; }

		public DataHandler<T> DataHandler { get; }

		public string Name => EntityDefine?.Name;

		public EntityExtractor()
		{
			EntityDefine = new EntityDefine<T>();
		}

		public EntityExtractor(DataHandler<T> dataHandler) : this()
		{
			DataHandler = dataHandler;
		}

		public virtual List<T> Extract(Page page)
		{
			List<T> result = new List<T>();
			if (EntityDefine.SharedValues != null && EntityDefine.SharedValues.Count > 0)
			{
				foreach (var enviromentValue in EntityDefine.SharedValues)
				{
					string name = enviromentValue.Name;
					var value = page.Selectable.Select(SelectorUtils.Parse(enviromentValue)).GetValue();
					page.Request.PutExtra(name, value);
				}
			}
			ISelector selector = SelectorUtils.Parse(EntityDefine.Selector);
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
				if (Env.IdColumns.Contains(field.Name))
				{
					continue;
				}
				var fieldValue = ExtractField(item, page, field, index);
				if (fieldValue == null)
				{
					if (field.NotNull)
					{
						skip = true;
						break;
					}
					else
					{
						field.Property.SetValue(dataObject, null);
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
			ISelector selector = SelectorUtils.Parse(field.Selector);
			if (selector == null)
			{
				return null;
			}

			if (selector is EnviromentSelector)
			{
				var enviromentSelector = selector as EnviromentSelector;
				var tmpValue = SelectorUtils.GetEnviromentValue(enviromentSelector.Field, page, index);
				foreach (var formatter in field.Formatters)
				{
#if DEBUG
					try
					{
#endif
						tmpValue = formatter.Formate(tmpValue);
#if DEBUG
					}
					catch (Exception e)
					{
					}
#endif
				}
				return tmpValue == null ? null : Convert.ChangeType(tmpValue, field.DataType);
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
						}
#endif
					}

					return value == null ? null : Convert.ChangeType(value, field.DataType);
				}
			}
		}
	}
}
