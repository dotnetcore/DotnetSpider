using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Extension.Common.Sql
{
	public class BaseInsert : Generatable<BaseInsert>
	{
		protected List<Pair> Pairs = new List<Pair>();

		public BaseInsert()
		{
		}

		public BaseInsert(string database, string table)
		{
			Database = database;
			Table = table;
		}

		public BaseInsert Values(string column, object value)
		{
			if (value != null)
				Pairs.Add(new Pair(column, value));
			return this;
		}

		public BaseInsert Values(bool ifTrue, string column, object value)
		{
			if (ifTrue)
				Values(column, value);
			return this;
		}

		public BaseInsert Values(Dictionary<string, object> map)
		{
			foreach (KeyValuePair<string, object> entry in map.AsEnumerable())
				Values(entry.Key, entry.Value);
			return this;
		}

		public override Command ToCommand()
		{
			Statement = $"USE {Database}; INSERT INTO {Table}({JoinNames(Pairs)}) VALUES ({JoinQuestionMarks(Pairs)})";
			Params = GetValues(Pairs);

			return new Command(Statement, Params);
		}
	}
}

