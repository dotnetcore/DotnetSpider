using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;

namespace Java2Dotnet.Spider.Validation
{
	public abstract class AbstractValidate : IValidate
	{
		protected string Arguments;
		protected DbConnection Connection;

		public string Sql { get; }
		public string Description { get; }
		public ValidateLevel Level { get; }

		protected AbstractValidate(DbConnection conn, string sql, string arguments, string description, ValidateLevel level)
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

			var command = Connection.CreateCommand();
			command.CommandText = Sql;
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
