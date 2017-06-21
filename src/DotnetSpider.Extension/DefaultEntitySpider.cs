namespace DotnetSpider.Extension
{
	public class DefaultEntitySpider : EntitySpider
	{
		public DefaultEntitySpider() : base(null, new Core.Site()) { }

		protected override void MyInit()
		{
		}
	}
}