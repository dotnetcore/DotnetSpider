using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;

namespace Java2Dotnet.Spider.Validation
{
	public abstract class AbstractValidate : IValidate
	{
		protected string Arguments;
		protected DbConnection Connection;

		public string Sql { get; }
		public string Description { get; }
		public ValidateLevel Level { get; }

		protected AbstractValidate(DbConnection conn, string sql, string arguments,  string description, ValidateLevel level)
		{
			Sql = sql;
			Arguments = arguments;
			Connection = conn;
			Description = description;
			Level = level;
		}

		public abstract ValidateResult Validate();

		public virtual void CheckArguments()
		{
			if (!Sql.ToLower().Contains("as result"))
			{
				throw new ValidationException("SQL should contains 'as result'");
			}
		}

		protected string GetValue()
		{
			if (Connection.State != ConnectionState.Open)
			{
				Connection.Open();
			}
			var result = Connection.Query<Value>(Sql).ToList().FirstOrDefault();
			return result?.Result;
		}

		protected List<Value> GetValueList()
		{
			if (Connection.State != ConnectionState.Open)
			{
				Connection.Open();
			}
			return Connection.Query<Value>(Sql).ToList();
		}

		public class Value
		{
			public string Result { get; set; }
		}
	}
}
