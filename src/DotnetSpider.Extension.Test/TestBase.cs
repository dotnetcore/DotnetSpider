namespace DotnetSpider.Extension.Test
{
	public abstract class TestBase
	{
		public virtual string DefaultConnectionString { get; } = "Database='mysql';Data Source=localhost;User ID=root;Port=3306;SslMode=None;";
	}
}
