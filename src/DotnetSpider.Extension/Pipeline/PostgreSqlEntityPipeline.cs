namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// 把解析到的爬虫实体数据存到PostgreSQL中
	/// </summary>
	public class PostgreSqlEntityPipeline : MySqlEntityPipeline
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">数据库连接字符串, 如果为空框架会尝试从配置文件中读取</param>
		public PostgreSqlEntityPipeline(string connectString = null) : base(connectString)
		{
		}
	}
}
