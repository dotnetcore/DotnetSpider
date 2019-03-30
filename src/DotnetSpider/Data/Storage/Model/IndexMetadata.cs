using System.Linq;

namespace DotnetSpider.Data.Storage.Model
{
    public class IndexMetadata
    {
        private readonly bool _isUnique;
        private readonly string _name;

        public IndexMetadata(string[] columns, bool isUnique = false)
        {
            Columns = columns;
            _isUnique = isUnique;
            _name = $"{(_isUnique ? "UNIQUE_" : "INDEX_")}{string.Join("_", columns.Select(x=>x.ToUpper()))}";
        }

        public string Name => _name;
        
        public bool IsUnique => _isUnique;

        public string[] Columns { get; }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }
    }
}