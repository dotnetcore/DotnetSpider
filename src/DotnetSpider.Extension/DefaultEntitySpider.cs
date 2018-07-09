using DotnetSpider.Core;

namespace DotnetSpider.Extension
{
	/// <summary>
	/// 用于测试
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