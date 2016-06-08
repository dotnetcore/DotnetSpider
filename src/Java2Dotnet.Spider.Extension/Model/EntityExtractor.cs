using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Selector;
using Java2Dotnet.Spider.Extension.Configuration;
using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Extension.Utils;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Model
{
	public class EntityExtractor : IEntityExtractor
	{
		private readonly Entity _entityDefine;
		private readonly List<EnviromentValue> _enviromentValues;

		public EntityExtractor(string entityName, List<EnviromentValue> enviromentValues, Entity entityDefine)
		{
			_entityDefine = entityDefine;

			EntityName = entityName;
			_enviromentValues = enviromentValues;
		}

		public dynamic Process(Page page)
		{
			if (_enviromentValues != null && _enviromentValues.Count > 0)
			{
				foreach (var enviromentValue in _enviromentValues)
				{
					string name = enviromentValue.Name;
					var value = page.Selectable.Select(SelectorUtil.GetSelector(enviromentValue.Selector)).GetValue();
					page.Request.PutExtra(name, value);
				}
			}
			bool isMulti = false;
			ISelector selector = SelectorUtil.GetSelector(_entityDefine.Selector);

			if (selector == null)
			{
				isMulti = false;
			}
			else
			{
				isMulti = _entityDefine.Multi;
			}
			if (isMulti)
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
					try
					{
						JObject obj = ProcessSingle(page, item, _entityDefine, index);
						if (obj != null)
						{
							result.Add(obj);
						}
						index++;
					}
					catch (Exception e)
					{

					}

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

				return ProcessSingle(page, select, _entityDefine, 0);
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
				return DateTimeUtils.MONDAY_RUN_ID;
			}

			if (field.ToLower() == "today")
			{
				return DateTimeUtils.TODAY_RUN_ID;
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

		private JObject ProcessSingle(Page page, ISelectable item, Entity entityDefine, int index)
		{
			JObject dataObject = new JObject();

			foreach (var field in entityDefine.Fields)
			{
				var fieldValue = ExtractField(item, page, field, index);
				if (fieldValue != null)
				{
					dataObject.Add(field.Name, fieldValue);
				}
			}

			var stopping = entityDefine.Stopping;

			if (stopping != null)
			{
				var field = entityDefine.Fields.First(f => f.Name == stopping.PropertyName);
				var datatype = field.DataType;
				bool isEntity = VerifyIfEntity(datatype);
				if (isEntity)
				{
					throw new SpiderExceptoin("Can't compare with object.");
				}
				stopping.DataType = datatype.ToString().ToLower();
				string value = dataObject.SelectToken($"$.{stopping.PropertyName}")?.ToString();
				if (string.IsNullOrEmpty(value))
				{
					page.MissTargetUrls = true;
				}
				else
				{
					if (stopping.NeedStop(value))
					{
						page.MissTargetUrls = true;
					}
				}
			}

			return dataObject.Children().Count() > 0 ? dataObject : null;
		}

		private dynamic ExtractField(ISelectable item, Page page, Field field, int index)
		{
			ISelector selector = SelectorUtil.GetSelector(field.Selector);
			if (selector == null)
			{
				return null;
			}

			List<Formatter.Formatter> formatters = GenerateFormatter(field.Formatters);

			bool isEntity = field.Fields != null && field.Fields.Count > 0;

			if (!isEntity)
			{
				string tmpValue;
				if (selector is EnviromentSelector)
				{
					var enviromentSelector = selector as EnviromentSelector;
					tmpValue = GetEnviromentValue(enviromentSelector.Field, page, index);
					foreach (var formatter in formatters)
					{
						tmpValue = formatter.Formate(tmpValue);
					}
					return tmpValue;
				}
				else
				{
					if (field.Multi)
					{
						var propertyValues = item.SelectList(selector).Nodes();
						if (field.Option == PropertyExtractBy.ValueOption.Count)
						{
							var tempValue = propertyValues != null ? propertyValues.Count.ToString() : "-1";
							return tempValue;
						}
						else
						{
							List<string> results = new List<string>();
							foreach (var propertyValue in propertyValues)
							{
								string tmp = propertyValue.GetValue(field.Option == PropertyExtractBy.ValueOption.PlainText);
								foreach (var formatter in formatters)
								{
									tmp = formatter.Formate(tmp);
								}
								results.Add(tmp);
							}
							return new JArray(results);
						}
					}
					else
					{
						tmpValue = item.Select(selector)?.GetValue(field.Option == PropertyExtractBy.ValueOption.PlainText);
						if (field.Option == PropertyExtractBy.ValueOption.Count)
						{
							return tmpValue == null ? 0 : 1;
						}
						else
						{
							tmpValue = formatters.Aggregate(tmpValue, (current, formatter) => formatter.Formate(current));
							return tmpValue;
						}
					}
				}
			}
			else
			{
				JObject dataObject = new JObject();
				foreach (var child in field.Fields)
				{
					if (child.Multi)
					{
						var childItems = item.SelectList(SelectorUtil.GetSelector(child.Selector)).Nodes();
						foreach (var childItem in childItems)
						{
							dataObject.Add(child.Name, ExtractField(childItem, page, child, childItems.IndexOf(childItem)));
						}
					}
					else
					{
						var childItem = item.Select(SelectorUtil.GetSelector(child.Selector));
						dataObject.Add(child.Name, ExtractField(childItem, page, child, 0));
					}
				}
				return dataObject;
			}
		}

		public static List<Formatter.Formatter> GenerateFormatter(IEnumerable<JToken> selectTokens)
		{
			if (selectTokens == null)
			{
				return new List<Formatter.Formatter>();
			}
			var results = new List<Formatter.Formatter>();
			foreach (var selectToken in selectTokens)
			{
				Type type = FormatterFactory.GetFormatterType(selectToken.SelectToken("$.Name")?.ToString());
				if (type != null)
				{
					results.Add(selectToken.ToObject(type) as Formatter.Formatter);
				}
			}
			return results;
		}

		private bool VerifyIfEntity(dynamic datatype)
		{
			return datatype != null && datatype.GetType() != typeof(string);
		}

		public string EntityName { get; }
	}
}
