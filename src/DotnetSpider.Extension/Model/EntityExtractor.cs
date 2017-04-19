using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Model
{
	public class EntityExtractor : IEntityExtractor
	{
		public EntityMetadata EntityMetadata { get; }
		public DataHandler DataHandler { get; set; }
		public string Name { get; }
		protected readonly List<SharedValueSelector> _globalValues;

		public EntityExtractor(string name, List<SharedValueSelector> globalValues, EntityMetadata entityDefine)
		{
			EntityMetadata = entityDefine;
			Name = name;
			DataHandler = entityDefine.DataHandler;
			_globalValues = globalValues;
		}

		public virtual List<JObject> Extract(Page page)
		{
			List<JObject> result = new List<JObject>();
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
						JObject obj = ExtractSingle(page, item, index);
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
					var singleResult = ExtractSingle(page, select, 0);
					result = new List<JObject> { singleResult };
				}
				else
				{
					result = null;
				}
			};
			return result;
		}

		private JObject ExtractSingle(Page page, ISelectable item, int index)
		{
			JObject dataObject = new JObject();

			bool skip = false;
			foreach (var field in EntityMetadata.Entity.Fields)
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
					dataObject.Add(field.Name, fieldValue);
				}
			}

			if (skip)
			{
				return null;
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
					var value = result.GetValue(targetUrl.PropertyName);
					if (value != null)
					{
						page.AddTargetRequest(new Request(value.ToString(), allExtras));
					}
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
					bool needPlainText = ((Field)field).Option == PropertySelector.Options.PlainText;
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
						bool needCount = (((Field)field).Option == PropertySelector.Options.Count);
						if (needCount)
						{
							var propertyValues = item.SelectList(selector).Nodes();
							string count = propertyValues?.Count.ToString();
							count = string.IsNullOrEmpty(count) ? "-1" : count;
							return count;
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
	}
}
