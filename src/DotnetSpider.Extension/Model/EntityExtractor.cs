using System;
using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Core.Common;
using DotnetSpider.Extension.Common;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Model
{
	public class EntityExtractor : IEntityExtractor
	{
		private readonly EntityMetadata _entityDefine;
		private readonly List<GlobalValueSelector> _globalValues;

		public EntityExtractor(string entityName, List<GlobalValueSelector> globalValues, EntityMetadata entityDefine)
		{
			_entityDefine = entityDefine;

			EntityName = entityName;
			_globalValues = globalValues;
		}

		public List<JObject> Process(Page page)
		{
			if (_globalValues != null && _globalValues.Count > 0)
			{
				foreach (var enviromentValue in _globalValues)
				{
					string name = enviromentValue.Name;
					var value = page.Selectable.Select(SelectorUtil.Parse(enviromentValue)).GetValue();
					page.Request.PutExtra(name, value);
				}
			}
			ISelector selector = SelectorUtil.Parse(_entityDefine.Entity.Selector);
			if (selector != null && _entityDefine.Entity.Multi)
			{
				var list = page.Selectable.SelectList(selector).Nodes();
				if (list == null || list.Count == 0)
				{
					return null;
				}
				var countToken = _entityDefine.Limit;
				if (countToken != null)
				{
					list = list.Take(countToken.Value).ToList();
				}

				List<JObject> result = new List<JObject>();
				int index = 0;
				foreach (var item in list)
				{
					JObject obj = ProcessSingle(page, item, _entityDefine, index);
					if (obj != null)
					{
						result.Add(obj);
					}
					index++;
				}
				return result;
			}
			else
			{
				ISelectable select;
				if (selector == null)
				{
					select = page.Selectable;
				}
				else
				{
					select = page.Selectable.Select(selector);
					if (select == null)
					{
						return null;
					}
				}

				var result = ProcessSingle(page, select, _entityDefine, 0);
				return result == null ? null : new List<JObject> { result };
			}
		}

		private string GetEnviromentValue(string field, Page page, int index)
		{
			if (field.ToLower() == "url")
			{
				return page.Url;
			}

			if (field.ToLower() == "targeturl")
			{
				return page.TargetUrl;
			}

			if (field.ToLower() == "now")
			{
				return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			}

			if (field.ToLower() == "monday")
			{
				return DateTimeUtils.MondayRunId;
			}

			if (field.ToLower() == "today")
			{
				return DateTimeUtils.TodayRunId;
			}

			if (field.ToLower() == "index")
			{
				return index.ToString();
			}

			if (!page.Request.ExistExtra(field))
			{
				return field;
			}
			else
			{
				return page.Request.GetExtra(field)?.ToString();
			}
		}

		private JObject ProcessSingle(Page page, ISelectable item, EntityMetadata entityDefine, int index)
		{
			JObject dataObject = new JObject();

			foreach (var field in entityDefine.Entity.Fields)
			{
				var fieldValue = ExtractField(item, page, field, index);
				if (fieldValue != null)
				{
					dataObject.Add(field.Name, fieldValue);
				}
			}

			return dataObject.Children().Any() ? dataObject : null;
		}

		private dynamic ExtractField(ISelectable item, Page page, DataToken field, int index)
		{
			ISelector selector = SelectorUtil.Parse(field.Selector);
			if (selector == null)
			{
				return null;
			}

			var f = field as Field;

			bool isEntity = field is Entity;

			if (!isEntity)
			{
				string tmpValue;
				if (selector is EnviromentSelector)
				{
					var enviromentSelector = selector as EnviromentSelector;
					tmpValue = GetEnviromentValue(enviromentSelector.Field, page, index);
					if (f != null)
					{
						foreach (var formatter in f.Formatters)
						{
							tmpValue = formatter.Formate(tmpValue);
						}
					}
					return tmpValue;
				}
				else
				{
					bool needPlainText = ((Field)field).Option == PropertySelector.ValueOption.PlainText;
					if (field.Multi)
					{
						var propertyValues = item.SelectList(selector).Nodes();

						List<string> results = new List<string>();
						foreach (var propertyValue in propertyValues)
						{
							string tmp = propertyValue.GetValue(needPlainText);
							if (f != null)
							{
								foreach (var formatter in f.Formatters)
								{
									tmp = formatter.Formate(tmp);
								}
							}
							results.Add(tmp);
						}
						return new JArray(results);
					}
					else
					{
						bool needCount = (((Field)field).Option == PropertySelector.ValueOption.Count);
						if (needCount)
						{
							var propertyValues = item.SelectList(selector).Nodes();
							return propertyValues?.Count.ToString() ?? "-1";
						}
						else
						{
							tmpValue = item.Select(selector)?.GetValue(needPlainText);
							if (f != null)
							{
								foreach (var formatter in f.Formatters)
								{
									tmpValue = formatter.Formate(tmpValue);
								}
							}
							return tmpValue;
						}
					}
				}
			}
			else
			{
				if (field.Multi)
				{
					JArray objs = new JArray();
					var selectables = item.SelectList(selector).Nodes();
					foreach (var selectable in selectables)
					{
						JObject obj = new JObject();

						foreach (var child in ((Entity)field).Fields)
						{
							obj.Add(child.Name, ExtractField(selectable, page, child, 0));
						}
						objs.Add(obj);
					}
					return objs;
				}
				else
				{
					JObject obj = new JObject();
					var selectable = item.Select(selector);
					foreach (var child in ((Entity)field).Fields)
					{
						obj.Add(child.Name, ExtractField(selectable, page, field, 0));
					}
					return obj;
				}
			}
		}

		//public static List<Formatter.Formatter> GenerateFormatter(IEnumerable<JToken> selectTokens)
		//{
		//	if (selectTokens == null)
		//	{
		//		return new List<Formatter.Formatter>();
		//	}
		//	var results = new List<Formatter.Formatter>();
		//	foreach (var selectToken in selectTokens)
		//	{
		//		Type type = FormatterFactory.GetFormatterType(selectToken.SelectToken("$.Name")?.ToString());
		//		if (type != null)
		//		{
		//			results.Add(selectToken.ToObject(type) as Formatter.Formatter);
		//		}
		//	}
		//	return results;
		//}

		public string EntityName { get; }
	}
}
