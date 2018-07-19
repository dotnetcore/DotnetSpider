using DotnetSpider.Common;

namespace DotnetSpider.Core
{
	public static class RequestExtensions
	{
		private const string CountOfResultsKey = "COUNT_OF_RESULTS_F0ADDCA1";
		private const string EffectedRowsKey = "EFFECTED_ROWS_8E9D9E82";

		public static int GetCountOfResults(this Request request)
		{
			if (request.Properties.ContainsKey(CountOfResultsKey))
			{
				return request.Properties[CountOfResultsKey];
			}
			else
			{
				return 0;
			}
		}

		public static int GetEffectedRows(this Request request)
		{
			if (request.Properties.ContainsKey(EffectedRowsKey))
			{
				return request.Properties[EffectedRowsKey];
			}
			else
			{
				return 0;
			}
		}

		public static void AddCountOfResults(this Request request, int count)
		{
			if (request.Properties.ContainsKey(CountOfResultsKey))
			{
				request.Properties[CountOfResultsKey] += count;
			}
			else
			{
				request.Properties.Add(CountOfResultsKey, count);
			}
		}

		public static void AddEffectedRows(this Request request, int count)
		{
			if (request.Properties.ContainsKey(EffectedRowsKey))
			{
				request.Properties[EffectedRowsKey] += count;
			}
			else
			{
				request.Properties.Add(EffectedRowsKey, count);
			}
		}
	}
}
