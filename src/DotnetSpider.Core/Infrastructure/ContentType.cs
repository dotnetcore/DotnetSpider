namespace DotnetSpider.Core.Infrastructure
{
	/// <summary>
	/// 下载的内容类型
	/// </summary>
	public enum ContentType
	{
		/// <summary>
		/// 由框架自动检测内容类型
		/// </summary>
		Auto,

		/// <summary>
		/// 下载的内容为HTML
		/// </summary>
		Html,

		/// <summary>
		/// 下载的内容为Json
		/// </summary>
		Json,

		/// <summary>
		/// 下载的内容为文件
		/// </summary>
		File
	}
}
