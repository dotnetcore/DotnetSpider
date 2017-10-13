namespace DotnetSpider.Extension.Pipeline
{
	public enum PipelineMode
	{
		/// <summary>
		///插数据, 不对主键或Unique重复做较验
		/// </summary>
		Insert,

		/// <summary>
		/// 只插入新数据, 如果主键或Unique有重复则忽略
		/// </summary>
		InsertAndIgnoreDuplicate,

		/// <summary>
		/// 插入新数据, 如果主键或Unique有重复则更新数据
		/// </summary>
		InsertNewAndUpdateOld,

		/// <summary>
		/// 更新数据
		/// </summary>
		Update
	}
}
