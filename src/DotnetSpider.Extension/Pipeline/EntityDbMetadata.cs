using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Pipeline
{
	public class EntityDbMetadata
	{
		public Table Table { get; set; }
		public List<Field> Columns { get; set; } = new List<Field>();
		public string InsertSql { get; set; }
		public string UpdateSql { get; set; }
		public string SelectSql { get; set; }
		public bool IsInsertModel { get; set; } = true;
	}
}
