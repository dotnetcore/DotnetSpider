namespace DotnetSpider.Data.Storage.Model
{
    public enum TablePostfix
    {
        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 表名的后缀为星期一的时间
        /// </summary>
        Monday,

        /// <summary>
        /// 表名的后缀为今天的时间 {name}_20171212
        /// </summary>
        Today,

        /// <summary>
        /// 表名的后缀为当月 {name}_201712
        /// </summary>
        Month
    }
}