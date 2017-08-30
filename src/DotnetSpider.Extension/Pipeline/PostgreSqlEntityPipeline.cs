using System.Data.Common;

namespace DotnetSpider.Extension.Pipeline
{
	public class PostgreSqlEntityPipeline : MySqlEntityPipeline
	{
		public PostgreSqlEntityPipeline(string connectString = null, bool checkIfSaveBeforeUpdate = false) : base(connectString, checkIfSaveBeforeUpdate)
		{
		}

		protected override DbParameter CreateDbParameter(string name, object value)
		{
			return new Npgsql.NpgsqlParameter(name, value);
		}
	}
}
