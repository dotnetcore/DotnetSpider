using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
