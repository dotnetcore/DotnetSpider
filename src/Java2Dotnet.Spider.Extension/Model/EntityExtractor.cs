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
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Model
{
	public class EntityExtractor : IEntityExtractor
	{
		private readonly JObject _entityDefine;

		public EntityExtractor(string entityName, JObject entityDefine)
		{
			_entityDefine = entityDefine;
			TargetUrlExtractInfos = GenerateTargetUrlExtractInfos(entityDefine);
			EntityName = entityName;
		}

		private List<TargetUrlExtractInfo> GenerateTargetUrlExtractInfos(JObject entityDefine)
		{
			List<TargetUrlExtractInfo> results = new List<TargetUrlExtractInfo>();
			var targetUrlTokens = entityDefine.SelectTokens("$.TargetUrls[*]");
			foreach (var targetUrlToken in targetUrlTokens)
			{
				var patterns = targetUrlToken.SelectToken("$.Values")?.ToObject<HashSet<string>>();
				var sourceregionToken = targetUrlToken.SelectToken("$.SourceRegion");
				results.Add(new TargetUrlExtractInfo()
				{
					Patterns = patterns == null || patterns.Count == 0 ? new List<Regex>() { new Regex("(.*)") } : patterns.Select(p => new Regex(p)).ToList(),
					TargetUrlRegionSelector = string.IsNullOrEmpty(sourceregionToken?.Value<string>()) ? null : Selectors.XPath(sourceregionToken.ToString())
				});
			}

			return results;
		}

		public dynamic Process(Page page)
		{
			bool isMulti = _entityDefine.SelectToken("$.Multi").ToObject<bool>();

			ISelector selector = GetSelector(_entityDefine.SelectToken("$.Selector").ToObject<Selector>());

			if (isMulti)
			{
				if (selector == null)
				{
					throw new SpiderExceptoin("Selector can't be null when set isMulti true.");
				}

				var list = page.Selectable.SelectList(selector).Nodes();
				var countToken = _entityDefine.SelectToken("$.Count");
				if (countToken != null)
				{
					int count = countToken.ToObject<int>();
					list = list.Take(count).ToList();
				}

				List<JObject> result = new List<JObject>();
				foreach (var item in list)
				{
					JObject obj = ProcessSingle(page, item, _entityDefine);
					if (obj != null)
					{
						result.Add(obj);
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

				return ProcessSingle(page, select, _entityDefine);
			}
		}

		private string GetEnviromentValue(string field, Page page)
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
				return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
			}

			if (field.ToLower() == "monday")
			{
				return DateTimeUtils.FirstDayofThisWeek.ToString("yyyy-MM-dd");
			}

			if (field.ToLower() == "today")
			{
				return DateTimeUtils.TodayRunId;
			}

			return page.Request.GetExtra(field);
		}

		private JObject ProcessSingle(Page page, ISelectable item, JToken entityDefine)
		{
			JObject dataItem = new JObject();

			foreach (var field in entityDefine.SelectTokens("$.Fields[*]"))
			{
				ISelector selector = GetSelector(field.SelectToken("$.Selector").ToObject<Selector>());
				if (selector == null)
				{
					continue;
				}

				var datatype = field.SelectToken("$.DataType");
				bool isEntity = VerifyIfEntity(datatype);

				var multiToken = field.SelectToken("$.Multi");
				bool isMulti = multiToken?.ToObject<bool>() ?? false;

				string propertyName = field.SelectToken("$.Name").ToString();

				List<Formatter.Formatter> formatters = GenerateFormatter(field.SelectTokens("$.Formatters[*]"));

				if (!isEntity)
				{
					string tmpValue;
					if (selector is EnviromentSelector)
					{
						var enviromentSelector = selector as EnviromentSelector;
						tmpValue = GetEnviromentValue(enviromentSelector.Field, page);
						foreach (var formatter in formatters)
						{
							tmpValue = formatter.Formate(tmpValue);
						}
						dataItem.Add(propertyName, tmpValue);
					}
					else
					{
						if (isMulti)
						{
							var propertyValues = item.SelectList(selector).Value;
							var countToken = _entityDefine.SelectToken("$.Count");
							if (countToken != null)
							{
								int count = countToken.ToObject<int>();
								propertyValues = propertyValues.Take(count).ToList();
							}
							List<string> results = new List<string>();
							foreach (var propertyValue in propertyValues)
							{
								string tmp = propertyValue;
								foreach (var formatter in formatters)
								{
									tmp = formatter.Formate(tmp);
								}
								results.Add(tmp);
							}
							dataItem.Add(propertyName, new JArray(results));
						}
						else
						{
							tmpValue = item.Select(selector)?.Value;
							tmpValue = formatters.Aggregate(tmpValue, (current, formatter) => formatter.Formate(current));
							dataItem.Add(propertyName, tmpValue);
						}
					}

				}
				else
				{
					if (isMulti)
					{
						var propertyValues = item.SelectList(selector).Nodes();
						var countToken = _entityDefine.SelectToken("$.Count");
						if (countToken != null)
						{
							int count = countToken.ToObject<int>();
							propertyValues = propertyValues.Take(count).ToList();
						}

						List<JObject> result = new List<JObject>();
						foreach (var entity in propertyValues)
						{
							JObject obj = ProcessSingle(page, entity, datatype);
							if (obj != null)
							{
								result.Add(obj);
							}
						}
						dataItem.Add(propertyName, new JArray(result));
					}
					else
					{
						var select = item.Select(selector);
						if (select == null)
						{
							return null;
						}
						var propertyValue = ProcessSingle(page, select, datatype);
						dataItem.Add(propertyName, new JObject(propertyValue));
					}
				}
			}
			var stoppingJobject = entityDefine.SelectToken("$.Stopping");
			var stopping = stoppingJobject?.ToObject<Stopping>();

			if (stopping != null)
			{
				var field = entityDefine.SelectToken($"$.Fields[?(@.Name == '{stopping.PropertyName}')]");
				var datatype = field.SelectToken("$.DataType");
				bool isEntity = VerifyIfEntity(datatype);
				if (isEntity)
				{
					throw new SpiderExceptoin("Can't compare with object.");
				}
				stopping.DataType = datatype.ToString().ToLower();
				string value = dataItem.SelectToken($"$.{stopping.PropertyName}")?.ToString();
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

			return dataItem;
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

		private bool VerifyIfEntity(JToken datatype)
		{
			return datatype.Type == JTokenType.Object;
		}

		/// <summary>
		/// 注意: 只有在Html页面中才能取得目标链接, 如果是Json数据, 一般是不会出现目标页面(下一页...）
		/// </summary>
		public List<TargetUrlExtractInfo> TargetUrlExtractInfos { get; }
		public string EntityName { get; }

		private static ISelector GetSelector(Selector selector)
		{
			if (string.IsNullOrEmpty(selector?.Expression))
			{
				return null;
			}

			string expression = selector.Expression;

			switch (selector.Type)
			{
				case ExtractType.Css:
					{
						return new CssHtmlSelector(expression);
					}
				case ExtractType.Enviroment:
					{
						return new EnviromentSelector(expression);
					}
				case ExtractType.JsonPath:
					{
						return new JsonPathSelector(expression);
					}
				case ExtractType.Regex:
					{
						return new RegexSelector(expression);
					}
				case ExtractType.XPath:
					{
						return new XPathSelector(expression);
					}
			}
			throw new SpiderExceptoin("Not support selector: " + selector);
		}
	}
}
