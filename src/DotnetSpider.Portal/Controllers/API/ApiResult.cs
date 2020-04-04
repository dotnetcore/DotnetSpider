namespace DotnetSpider.Portal.Controllers.API
{
	public interface IApiResult
	{
		bool Success { get; }
		int Code { get; }
		string Msg { get; }
	}

	public class ApiResult : ApiResult<object>
	{
		public ApiResult(object data, string msg = null) : base(data, msg)
		{
		}
	}

	public class ApiResult<T> : IApiResult
	{
		public bool Success { get; }
		public int Code { get; }
		public string Msg { get; }
		public T Data { get; }

		public ApiResult(T data, string msg = null)
		{
			Success = true;
			Code = 0;
			Msg = msg;
			Data = data;
		}
	}

	public class FailedResult : IApiResult
	{
		public bool Success { get; }
		public int Code { get; }
		public string Msg { get; }

		public FailedResult(string msg, int code = 1)
		{
			Success = false;
			Code = code;
			Msg = msg;
		}
	}
}
