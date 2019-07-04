namespace DotnetSpider.EventBus
{
	/// <summary>
	/// 命令消息
	/// </summary>
	public class Event
	{
		/// <summary>
		/// 命令
		/// </summary>
		public string Command { get; set; }

		/// <summary>
		/// 消息
		/// </summary>
		public string Message { get; set; }
	}
}