namespace DotnetSpider.Extension.Pipeline
{
	public class PostgreSqlEntityPipeline : MySqlEntityPipeline
	{
		public PostgreSqlEntityPipeline(string connectString = null) : base(connectString)
		{
		}
	}
}
