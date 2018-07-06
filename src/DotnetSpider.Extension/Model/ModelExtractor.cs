using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Infrastructure;
using System;
using Serilog;
using System.Reflection;

namespace DotnetSpider.Extension.Model
{
	public class ModelExtractor : IModelExtractor
	{
		/// <summary>
		/// 解析成爬虫实体对象
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <param name="model">解析模型</param>
		/// <returns>爬虫实体对象</returns>
		public virtual IEnumerable<dynamic> Extract(Page page, IModel model)
		{
			List<dynamic> result = new List<dynamic>();
			if (model.SharedValueSelectors != null)
			{
				foreach (var enviromentValue in model.SharedValueSelectors)
				{
					string name = enviromentValue.Name;
					var value = page.Selectable.Select(enviromentValue.ToSelector()).GetValue();
					page.Request.PutExtra(name, value);
				}
			}

			bool singleExtractor = model.Selector == null;

			if (!singleExtractor)
			{
				var selector = model.Selector.ToSelector();

				var list = page.Selectable.SelectList(selector).Nodes()?.ToList();
				if (list == null)
				{
					result = null;
				}
				else
				{
					if (model.Take > 0 && list.Count > model.Take)
					{
						if (model.TakeFromHead)
						{
							list = list.Take(model.Take).ToList();
						}
						else
						{
							list = list.Skip(list.Count - model.Take).ToList();
						}
					}

					for (int i = 0; i < list.Count; ++i)
					{
						var item = list.ElementAt(i);
						var obj = ExtractSingle(page, model, item, i);
						if (obj != null)
						{
							result.Add(obj);
						}
					}
				}
			}
			else
			{
				var obj = ExtractSingle(page, model, page.Selectable, 0);
				if (obj != null)
				{
					result.Add(obj);
				}
			}

			return result;
		}

		private Dictionary<string, dynamic> ExtractSingle(Page page, IModel model, ISelectable item, int index)
		{
			Dictionary<string, dynamic> dataObject = new Dictionary<string, dynamic>();

			foreach (var field in model.Fields)
			{
				var fieldValue = ExtractField(item, page, field, index);
				if (fieldValue == null && field.NotNull)
				{
					return null;
				}
				else
				{
					dataObject.Add(field.Name, fieldValue);
				}
			}

			return dataObject;

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

		private string ExtractField(ISelectable item, Page page, Field field, int index)
		{
			if (field == null)
			{
				return null;
			}

			var selector = field.ToSelector();
			if (selector == null)
			{
				return null;
			}

			object value;
			if (selector is EnviromentSelector)
			{
				var enviromentSelector = selector as EnviromentSelector;
				value = SelectorUtil.GetEnviromentValue(enviromentSelector.Field, page, index);
			}
			else
			{
				value = field.Option == FieldOptions.Count
					? item.SelectList(selector).Nodes().Count().ToString()
					: item.Select(selector)?.GetValue(ConvertToValueOption(field.Option));
			}

			if (field.Formatters != null && field.Formatters.Count() > 0)
			{
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
						Log.Logger.Error(e.ToString());
					}
#endif
				}
			}

			return value?.ToString();
		}

		private ValueOption ConvertToValueOption(FieldOptions options)
		{
			switch (options)
			{
				case FieldOptions.InnerHtml:
				{
					return ValueOption.InnerHtml;
				}
				case FieldOptions.OuterHtml:
				{
					return ValueOption.OuterHtml;
				}
				case FieldOptions.InnerText:
				{
					return ValueOption.InnerText;
				}
				default:
				{
					return ValueOption.None;
				}
			}
		}
	}

	public class ModelExtractor<T> : ModelExtractor where T : new()
	{
		private readonly Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();

		public ModelExtractor()
		{
			var properties = typeof(T).GetProperties();
			foreach (var prop in properties)
			{
				_properties.Add(prop.Name, prop);
			}
		}

		public override IEnumerable<dynamic> Extract(Page page, IModel model)
		{
			var items = base.Extract(page, model)?.ToList();

			if (items == null)
			{
				return new List<dynamic>();
			}

			List<dynamic> results = new List<dynamic>();
			foreach (var item in items)
			{
				var o = Activator.CreateInstance<T>();
				foreach (var keyPair in _properties)
				{
					if (item.ContainsKey(keyPair.Key))
					{
						var oldValue = item[keyPair.Key];
						var valueType = keyPair.Value.PropertyType;
						try
						{
							if (oldValue != null)
							{
								var newValue = Convert.ChangeType(oldValue, valueType);
								if (newValue != null)
								{
									keyPair.Value.SetValue(o, newValue);
								}
							}
						}
						catch
						{
							Log.Logger.Debug($"Convert data {oldValue} to {valueType.FullName} failed.");
						}
					}
				}

				results.Add(o);
			}

			return results;
		}
	}
}