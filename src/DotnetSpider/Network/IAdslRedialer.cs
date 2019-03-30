namespace DotnetSpider.Network
{
	/// <summary>
	/// 拨号器
	/// </summary>
	public interface IAdslRedialer
	{
		/// <summary>
		/// 拨号
		/// </summary>
		bool Redial();
	}
}
