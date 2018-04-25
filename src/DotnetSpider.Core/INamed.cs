namespace DotnetSpider.Core
{
	/// <summary>
	/// 名称接口定义
	/// </summary>
	public interface INamed
	{
		/// <summary>
		/// 名称
		/// </summary>
		string Name { get; set; }
	}

	/// <summary>
	/// 名称的抽象
	/// </summary>
	public abstract class Named : INamed
	{
		/// <summary>
		/// 名称
		/// </summary>
		public string Name { get; set; }
	}
}
