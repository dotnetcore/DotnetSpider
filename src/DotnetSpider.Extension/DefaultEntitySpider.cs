using DotnetSpider.Core;

namespace DotnetSpider.Extension
{
	/// <summary>
	/// ÓÃÓÚ²âÊÔ
	/// </summary>
	internal class DefaultEntitySpider : EntitySpider
	{
		internal DefaultEntitySpider() : this(new Site()) { }

		internal DefaultEntitySpider(Site site) : base(null, site)
		{
		}

		protected override void MyInit(params string[] arguments)
		{
		}
	}
}