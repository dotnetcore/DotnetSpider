namespace DotnetSpider.DataFlow
{
	/// <summary>
	/// 数据流处理器的处理结果
	/// </summary>
	public enum DataFlowResult
	{
		/// <summary>
		/// 数据处理正常结束
		/// </summary>
		Success,

		/// <summary>
		/// 数据处理异常结束，会终止数据的流转
		/// </summary>
		Failed,

		/// <summary>
		/// 数据处理结束，表示如果存在数据流转则不需要再流转下去
		/// </summary>
		Terminated
	}
}