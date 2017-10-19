using System.Data.Common;

namespace DotnetSpider.Extension.Pipeline
{
	public sealed class PostgreSqlEntityPipeline : MySqlEntityPipeline
	{
		public PostgreSqlEntityPipeline(string connectString = null) : base(connectString)
		{
		}
	}
}
