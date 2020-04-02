namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 存储器类型
	/// </summary>
    public enum StorageMode
    {
        /// <summary>
        /// 直接执行插入
        /// </summary>
        Insert,

        /// <summary>
        /// 插入不重复数据
        /// </summary>
        InsertIgnoreDuplicate,

        /// <summary>
        /// 如果主键不存在则插入, 如果存在则更新
        /// </summary>
        InsertAndUpdate,

        /// <summary>
        /// 直接更新
        /// </summary>
        Update
    }
}