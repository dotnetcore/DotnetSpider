using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration
{
	public class SpiderContextValidation
	{
		public static bool Validate(Json.JsonSpiderContext spiderContext, out List<string> messages)
		{
			bool correct = true;
			messages = new List<string>();

			// 1. 脚本未能正确序列化
			if (spiderContext == null)
			{
				correct = false;
				messages.Add("Error 001: Script can't deserialize to a JsonSpider object.");
			}
			else
			{
				if (spiderContext.Entities == null || spiderContext.Entities.Count == 0)
				{
					messages.Add($"Error 002: Didn't define any data entity.");
				}
				else
				{
					// 2. 如果数据实体有嵌套或者属性是List, 则Pipeline只能选择mongodb.
					foreach (var entity in spiderContext.Entities)
					{
						var fieldTokens = entity.Fields;
						string entityName = entity.Identity;

						if (string.IsNullOrEmpty(entityName))
						{
							if (correct)
							{
								correct = false;
							}
							messages.Add($"Error 003: Entity.Identity is null.");
						}

						if (fieldTokens == null || fieldTokens.Count == 0)
						{
							if (correct)
							{
								correct = false;
							}

							messages.Add($"Error 004: Entity: {entityName} has no field.");
						}

						if (fieldTokens != null)
						{
							foreach (var fieldToken in fieldTokens)
							{
								string fieldName = fieldToken.Name;
								if (string.IsNullOrEmpty(fieldName))
								{
									messages.Add($"Error 005: Entity: {entityName}, Field index: {fieldTokens.IndexOf(fieldToken)} has no name.");
								}

								var dataTypeToken = fieldToken.DataType;
								if (dataTypeToken != null)
								{
									if (dataTypeToken.Type != JTokenType.String && !(spiderContext.Pipeline.SelectToken("$.Type").ToString() == "2" || spiderContext.Pipeline.SelectToken("$.Type").ToString() == "3"))
									{
										if (correct)
										{
											correct = false;
										}
										messages.Add($"Error 007: Entity: {entityName}, Field index: {fieldTokens.IndexOf(fieldToken)} is a class, when data entity is a loop type, pipeline should be mongodb only.");
									}
								}
							}
						}
					}
				}
			}

			return correct;
		}
	}
}
