using DotnetSpider.Core;

namespace DotnetSpider.Extension
{
	public class DefaultEntitySpider : EntitySpider
	{
		public DefaultEntitySpider() : this(new Site()) { }

		public DefaultEntitySpider(Site site) : base(null, site)
		{
			Core.Infrastructure.Database.DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);
		}

		protected override void MyInit(params string[] arguments)
		{
		}
	}
}