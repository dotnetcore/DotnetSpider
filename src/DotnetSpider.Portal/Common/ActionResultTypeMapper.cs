using System;
using DotnetSpider.Portal.Controllers.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace DotnetSpider.Portal.Common
{
	public class ActionResultTypeMapper : IActionResultTypeMapper
	{
		public Type GetResultDataType(Type returnType)
		{
			if (returnType == null)
				throw new ArgumentNullException(nameof(returnType));
			return returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>)
				? returnType.GetGenericArguments()[0]
				: returnType;
		}

		public IActionResult Convert(object value, Type returnType)
		{
			if (returnType == null)
				throw new ArgumentNullException(nameof(returnType));
			switch (value)
			{
				case IConvertToActionResult convertToActionResult:
					return convertToActionResult.Convert();
				case IApiResult _:
					return new JsonResult(value);
				default:
					return new JsonResult(new ApiResult(value));
			}
		}
	}
}
