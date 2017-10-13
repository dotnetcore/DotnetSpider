using DotnetSpider.Extension.Model;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public class EntityAdapter
	{
		public EntityTable Table { get; set; }

		public List<Column> Columns { get; set; }

		internal string InsertSql { get; set; }

		internal string InsertAndIgnoreDuplicateSql { get; set; }

		internal string UpdateSql { get; set; }

		internal string SelectSql { get; set; }

		internal string InsertNewAndUpdateOldSql { get; set; }

		public PipelineMode PipelineMode { get; set; } = PipelineMode.InsertAndIgnoreDuplicate;

		public EntityAdapter(EntityTable table, List<Column> columns)
		{
			Table = table;
			Columns = columns;
		}
	}
}
