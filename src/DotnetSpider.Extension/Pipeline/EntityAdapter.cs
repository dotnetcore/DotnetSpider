using DotnetSpider.Extension.Model;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public class EntityAdapter
	{
		public EntityTable Table { get; set; }

		public List<Column> Columns { get; set; }

		public string InsertSql { get; set; }

		public string UpdateSql { get; set; }

		public string SelectSql { get; set; }

		public bool InsertModel { get; set; } = true;

		public EntityAdapter(EntityTable table, List<Column> columns)
		{
			Table = table;
			Columns = columns;
		}
	}
}
