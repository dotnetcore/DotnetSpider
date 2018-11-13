using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension.Model
{
	public abstract class BaseEntity : IBaseEntity
	{
		[Column]
		[Primary]
		public int Id { get; set; }
	}
}
