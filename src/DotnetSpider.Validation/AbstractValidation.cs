using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;

namespace DotnetSpider.Validation
{
	public abstract class AbstractValidation : IValidation
	{
		public string Arguments { get; set; }
		public string Sql { get; set; }
		public string Description { get; set; }
		public ValidateLevel Level { get; set; }

		protected DbConnection Connection;

		protected AbstractValidation(DbConnection conn, string sql, string arguments, string description, ValidateLevel level)
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
			return GetValue(Sql);
		}

		protected string GetValue(string query)
		{
			if (Connection.State != ConnectionState.Open)
			{
				Connection.Open();
			}

			var command = Connection.CreateCommand();
			command.CommandText = query;
			command.CommandType = CommandType.Text;
			var reader = command.ExecuteReader();
			object result = new object();
			while (reader.Read())
			{
				result = reader["result"];
			}
#if !NET_CORE
			reader.Close();
#else
			reader.Dispose();
#endif
			Connection.Close();
			return result?.ToString();
		}

		protected List<Value> GetValueList()
		{
			if (Connection.State != ConnectionState.Open)
			{
				Connection.Open();
			}
			List<Value> values = new List<Value>();
			var command = Connection.CreateCommand();
			command.CommandText = Sql;
			command.CommandType = CommandType.Text;
			var reader = command.ExecuteReader();

			while (reader.Read())
			{
				var result = reader["result"];
				if (result != null)
				{
					values.Add(new Value() { Result = result.ToString() });
				}
			}
#if !NET_CORE
			reader.Close();
#else
			reader.Dispose();
#endif
			Connection.Close();
			return values;
		}

		public class Value
		{
			public string Result { get; set; }
		}
	}
}
