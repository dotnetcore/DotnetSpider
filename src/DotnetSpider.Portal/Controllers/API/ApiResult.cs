using Microsoft.AspNetCore.Mvc;

namespace DotnetSpider.Portal.Controllers.API
{
	public interface IApiResult
	{
	}

	public class ApiResult : JsonResult, IApiResult
	{
		public ApiResult(object value) : base(new {code = 0, success = true, data = value})
		{
		}

		public ApiResult(object value, object serializerSettings) : base(new {code = 0, success = true, data = value},
			serializerSettings)
		{
		}
	}

	public class FailedResult : JsonResult, IApiResult
	{
		public FailedResult(string msg) : base(new {code = 1, success = false, msg})
		{
		}

		public FailedResult(string msg, object serializerSettings) : base(new {code = 1, success = false, msg},
			serializerSettings)
		{
		}
	}
}
