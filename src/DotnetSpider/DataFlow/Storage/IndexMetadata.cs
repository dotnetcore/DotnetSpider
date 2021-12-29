using System.Linq;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 索引元数据
	/// </summary>
    public class IndexMetadata
    {
        private readonly bool _isUnique;
        private readonly string _name;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="columns">列</param>
        /// <param name="isUnique">是否唯一索引</param>
        public IndexMetadata(string[] columns, bool isUnique = false)
        {
            Columns = columns;
            _isUnique = isUnique;
            _name = $"{(_isUnique ? "UNIQUE_" : "INDEX_")}{string.Join("_", columns.Select(x=>x.ToUpper()))}";
        }

        /// <summary>
        /// 索引名称
        /// </summary>
        public string Name => _name;
        
        /// <summary>
        /// 是否唯一索引
        /// </summary>
        public bool IsUnique => _isUnique;

        /// <summary>
        /// 索引的列
        /// </summary>
        public string[] Columns { get; }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }
    }
}