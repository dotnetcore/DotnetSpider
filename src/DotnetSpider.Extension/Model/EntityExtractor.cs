using System;
using System.Collections.Generic;
using System.Linq;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif
using System.Text.RegularExpressions;
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
		public EntityMetadata EntityMetadata { get; }
		public DataHandler DataHandler { get; set; }
		private readonly List<GlobalValueSelector> _globalValues;

		public EntityExtractor(string entityName, List<GlobalValueSelector> globalValues, EntityMetadata entityDefine)
		{
			EntityMetadata = entityDefine;
			EntityName = entityName;
			DataHandler = entityDefine.DataHandler;
			_globalValues = globalValues;
		}

		public List<JObject> Process(Page page)
		{
			List<JObject> result = new List<JObject>();
			bool isTarget = true;
			foreach (var targetUrlExtractor in EntityMetadata.TargetUrlExtractors)
			{
				foreach (var regex in targetUrlExtractor.Regexes)
				{
					isTarget = regex.IsMatch(page.Url);
					if (isTarget)
					{
						break;
					}
				}
			}
			if (!isTarget)
			{
				return null;
			}
			if (_globalValues != null && _globalValues.Count > 0)
			{
				foreach (var enviromentValue in _globalValues)
				{
					string name = enviromentValue.Name;
					var value = page.Selectable.Select(SelectorUtil.Parse(enviromentValue)).GetValue();
					page.Request.PutExtra(name, value);
				}
			}
			ISelector selector = SelectorUtil.Parse(EntityMetadata.Entity.Selector);
			if (selector != null && EntityMetadata.Entity.Multi)
			{
				var list = page.Selectable.SelectList(selector).Nodes();
				if (list == null || list.Count == 0)
				{
					result = null;
				}
				else
				{
					var countToken = EntityMetadata.Limit;
					if (countToken != null)
					{
						list = list.Take(countToken.Value).ToList();
					}

					int index = 0;
					foreach (var item in list)
					{
						JObject obj = ProcessSingle(page, item, index);
						if (obj != null)
						{
							result.Add(obj);
						}
						index++;
					}
				}
			}
			else
			{
				ISelectable select = selector == null ? page.Selectable : page.Selectable.Select(selector);

				if (select != null)
				{
					var singleResult = ProcessSingle(page, select, 0);
					result = new List<JObject> { singleResult };
				}
				else
				{
					result = null;
				}
			}

			//if (EntityMetadata.TargetUrlsCreators != null && EntityMetadata.TargetUrlExtractors.Count > 0)
			//{
			//	foreach (var targetUrlsCreator in EntityMetadata.TargetUrlsCreators)
			//	{
			//		page.AddTargetRequests(targetUrlsCreator.Handle(page));
			//	}
			//}
			
			if(!page.MissExtractTargetUrls)
			{
				ExtractLinks(page, EntityMetadata.TargetUrlExtractors);
			}

			return result;
		}

		/// <summary>
		/// 如果找不到则不返回URL, 不然返回的URL太多
		/// </summary>
		/// <param name="page"></param>
		/// <param name="targetUrlExtractInfos"></param>
		private void ExtractLinks(Page page, List<TargetUrlExtractor> targetUrlExtractInfos)
		{
			if (targetUrlExtractInfos == null)
			{
				return;
			}

			foreach (var targetUrlExtractInfo in targetUrlExtractInfos)
			{
				var urlRegionSelector = targetUrlExtractInfo.RegionSelector;
				var formatters = targetUrlExtractInfo.Formatters;
				var urlPatterns = targetUrlExtractInfo.Regexes;

				var links = urlRegionSelector == null ? page.Selectable.Links().GetValues() : (page.Selectable.SelectList(urlRegionSelector)).Links().GetValues();
				if (links == null)
				{
					return;
				}

				// check: 仔细考虑是放在前面, 还是在后面做 formatter, 我倾向于在前面. 对targetUrl做formatter则表示Start Url也应该是要符合这个规则的。
				if (formatters != null && formatters.Count > 0)
				{
					List<string> tmp = new List<string>();
					foreach (string link in links)
					{
						var url = new String(link.ToCharArray());
						foreach (Formatter.Formatter f in formatters)
						{
							url = f.Formate(url);
						}
						tmp.Add(url);
					}
					links = tmp;
				}

				List<string> tmpLinks = new List<string>();
				foreach (var link in links)
				{
#if !NET_CORE
					tmpLinks.Add(HttpUtility.HtmlDecode(HttpUtility.UrlDecode(link)));
#else
					tmpLinks.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(link)));
#endif
				}
				links = tmpLinks;

				if (urlPatterns == null || urlPatterns.Count == 0)
				{
					page.AddTargetRequests(links);
					return;
				}

				foreach (Regex targetUrlPattern in urlPatterns)
				{
					foreach (string link in links)
					{
						if (targetUrlPattern.IsMatch(link))
						{
							page.AddTargetRequest(new Request(link, page.Request.NextDepth, page.Request.Extras));
						}
					}
				}
			}
		}

		public static string GetEnviromentValue(string field, Page page, int index)
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
				return DateTimeUtils.RunIdOfMonday;
			}

			if (field.ToLower() == "today")
			{
				return DateTimeUtils.RunIdOfToday;
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

		private JObject ProcessSingle(Page page, ISelectable item, int index)
		{
			JObject dataObject = new JObject();

			foreach (var field in EntityMetadata.Entity.Fields)
			{
				var fieldValue = ExtractField(item, page, field, index);
				if (fieldValue != null)
				{
					dataObject.Add(field.Name, fieldValue);
				}
			}

			var result = dataObject.Children().Any() ? dataObject : null;
			if (result != null)
			{
				foreach (var targetUrl in EntityMetadata.Entity.TargetUrls)
				{
					Dictionary<string, dynamic> extras = new Dictionary<string, dynamic>();
					if (targetUrl.Extras != null)
					{
						foreach (var extra in targetUrl.Extras)
						{
							extras.Add(extra, result.GetValue(extra));
						}
					}
					Dictionary<string, dynamic> allExtras = new Dictionary<string, dynamic>();
					foreach (var extra in page.Request.Extras.Union(extras))
					{
						allExtras.Add(extra.Key, extra.Value);
					}
					page.AddTargetRequest(new Request(result.GetValue(targetUrl.PropertyName).ToString(), page.Request.NextDepth, allExtras));
				}

			}
			return result;
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
							results.Add(propertyValue.GetValue(needPlainText));
						}
						if (f != null)
						{
							foreach (var formatter in f.Formatters)
							{
								results = formatter.Formate(results);
							}
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

		public string EntityName { get; }
	}
}
