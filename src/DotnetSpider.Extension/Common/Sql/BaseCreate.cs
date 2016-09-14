using System.Collections.Generic;

namespace DotnetSpider.Extension.Common.Sql
{
	public class BaseCreate : Generatable<BaseCreate>
	{
		protected List<Pair> Columns = new List<Pair>();
		protected internal bool DropIfExists;

		public BaseCreate()
		{
		}

		public BaseCreate(string database, string table, bool drop = false)
		{
			Table = table;
			DropIfExists = drop;
			Database = database;
		}

		public override Command ToCommand()
		{
			if (DropIfExists)
			{
				Statement = ($@"USE {Database}; IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{Table}') AND type in (N'U')) DROP TABLE {Table}");
			}
			Statement += $@"USE {Database}; CREATE TABLE {Table}({GenerateColumns()}) ON [PRIMARY]";
			return new Command(Statement, Params);
		}

		private string GenerateColumns()
		{
			string statement = "";

			for (int i = 0, updatesSize = Columns.Count; i < updatesSize; i++)
			{
				Pair pair = Columns[i];
				if (pair is StatementPair)
					statement += pair.Name;
				else
				{
					Params.Add(pair.Value.ToString());
					statement += $"[{pair.Name}] {pair.Value}";
				}

				if (i < updatesSize - 1)
				{
					statement += ",";
				}
			}

			return statement;
		}

		public BaseCreate AddColumn(string column, string typeName, string identityString = null, string size = "", bool nullable = true)
		{
			string type = typeName;
			type = string.IsNullOrEmpty(size) ? type : $"{type}({size})";
			type = string.IsNullOrEmpty(identityString) ? type : $"{type} {identityString}";
			type = nullable ? $"{type} NULL" : $"{type} NOT NULL";
			Columns.Add(new Pair(column, type));
			return this;
		}
	}
}
