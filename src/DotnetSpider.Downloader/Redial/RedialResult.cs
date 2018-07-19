namespace DotnetSpider.Downloader.Redial
{
	/// <summary>
	/// 拨号结果
	/// </summary>
	public enum RedialResult
	{
		/// <summary>
		/// 拨号失败
		/// </summary>
		Failed,
		/// <summary>
		/// 拨号成功
		/// </summary>
		Sucess,
		/// <summary>
		/// 此次拨号跳过
		/// </summary>
		Skip,
		/// <summary>
		/// 其它程序已执行过拨号
		/// </summary>
		OtherRedialed
	}
}
