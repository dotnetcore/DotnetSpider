using DotnetSpider.Core;

namespace DotnetSpider.Extension
{
	public class DefaultEntitySpider : EntitySpider
	{
		public DefaultEntitySpider() : this(new Site()) { }

		public DefaultEntitySpider(Site site) : base(null, site) { }

		protected override void MyInit(params string[] arguments)
		{
		}
	}
}