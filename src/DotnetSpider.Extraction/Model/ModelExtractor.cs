using System.Collections.Generic;
using System.Linq;
using DotnetSpider.Extraction.Model.Attribute;
using System;
using System.Reflection;
using System.Diagnostics;

namespace DotnetSpider.Extraction.Model
{
	public class ModelExtractor : IModelExtractor
	{
		/// <summary>
		/// 解析成实体对象
		/// </summary>
		/// <param name="selectable">可查询对象</param>
		/// <param name="model">解析模型</param>
		/// <returns>实体对象</returns>
		public virtual IList<dynamic> Extract(Selectable selectable, IModel model)
		{
			List<dynamic> results = new List<dynamic>();
			if (selectable.Properties == null)
			{
				selectable.Properties = new Dictionary<string, object>();
			}

			if (model.SharedValueSelectors != null)
			{
				foreach (var enviromentValue in model.SharedValueSelectors)
				{
					string name = enviromentValue.Name;
					var value = selectable.Select(enviromentValue.ToSelector()).GetValue();
					if (!selectable.Properties.ContainsKey(name))
					{
						selectable.Properties.Add(name, value);
					}
				}
			}

			bool singleExtractor = model.Selector == null;

			if (!singleExtractor)
			{
				var selector = model.Selector.ToSelector();

				var list = selectable.SelectList(selector).Nodes()?.ToList();
				if (list == null)
				{
					results = null;
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
						var obj = ExtractObject(model, item, selectable, i);
						if (obj != null)
						{
							results.Add(obj);
						}
					}
				}
			}
			else
			{
				var obj = ExtractObject(model, selectable, selectable, 0);
				if (obj != null)
				{
					results.Add(obj);
				}
			}

			return results;
		}

		private Dictionary<string, dynamic> ExtractObject(IModel model, ISelectable obj, Selectable root, int index)
		{
			Dictionary<string, dynamic> dataObject = new Dictionary<string, dynamic>();

			foreach (var field in model.Fields)
			{
				var fieldValue = ExtractField(field, obj, root, index);
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
		}

		private string ExtractField(FieldSelector field, ISelectable item, Selectable root, int index)
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
				switch (enviromentSelector.Field)
				{
					case EnviromentFields.Index:
						{
							value = index;
							break;
						}
					default:
						{
							value = root.Enviroment(enviromentSelector.Field);
							break;
						}
				}
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
						Debugger.Log(0, "ERROR", $"ModelExtractor execute formatter failed: {e}");
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

		public override IList<dynamic> Extract(Selectable response, IModel model)
		{
			var items = base.Extract(response, model)?.ToList();

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
									keyPair.Value.SetValue(o, newValue, new object[0]);
								}
							}
						}
						catch
						{
							throw new ExtractionException($"Convert data {oldValue} to {valueType.Name} failed.");
						}
					}
				}

				results.Add(o);
			}

			return results;
		}
	}
}