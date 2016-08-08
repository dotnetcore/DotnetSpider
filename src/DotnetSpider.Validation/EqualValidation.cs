using System;
using System.Data.Common;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Validation
{
	public class EqualValidation : AbstractValidation
	{
		public EqualValidation(DbConnection conn, string sql, string arguments, string description, ValidateLevel level = ValidateLevel.Error) : base(conn, sql, arguments, description, level)
		{
		}

		public override ValidateResult Validate()
		{
			try
			{
				string value = GetValue();
				string result = "";
				if (Arguments.Contains("select") && Arguments.Contains("from") && Arguments.Contains("as result"))
				{
					result = GetValue(Arguments);
				}
				else
				{
					result = Arguments;
				}
				return new ValidateResult
				{
					IsPass = value == result,
					Arguments = Arguments,
					Description = Description,
					Sql = Sql,
					ActualValue = value
				};
			}
			catch (Exception e)
			{
				return new ValidateResult
				{
#if !NET_CORE
					Message = HttpUtility.HtmlAttributeEncode(e.Message).Replace('\"', '\0'),
#else
					Message = WebUtility.HtmlEncode(e.Message).Replace('\"', '\0'),
#endif
					IsPass = false,
					Arguments = Arguments,
					Description = Description,
					Sql = Sql
				};
			}
		}

		public override string ToString()
		{
			return $"CompareTo: {Arguments}";
		}
	}
}
