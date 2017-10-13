using System.Data.Common;

namespace DotnetSpider.Extension.Pipeline
{
	public class PostgreSqlEntityPipeline : MySqlEntityPipeline
	{
		public PostgreSqlEntityPipeline(string connectString = null) : base(connectString)
		{
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			return new Npgsql.NpgsqlParameter(name, value);
		}
	}
}
