using System;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Common.Sql
{
	/// <summary>
	/// 用于生成 update 语句的帮助类
	/// </summary>
	public class BaseUpdate : Generatable<BaseUpdate>
	{
		protected List<Pair> Updates = new List<Pair>();

		public BaseUpdate()
		{
		}

		public BaseUpdate(string database,string table)
		{
			Database = database;
			Table = table;
		}

		public override Command ToCommand()
		{
			Statement = $"USE {Database}; UPDATE {Table} set {GenerateSetBlock()} {GenerateWhereBlock()}";

			return new Command(Statement, Params);
		}

		private string GenerateSetBlock()
		{
			string statement = "";

			for (int i = 0, updatesSize = Updates.Count; i < updatesSize; i++)
			{
				Pair pair = Updates[i];
				if (pair is StatementPair)
				{
					statement += pair.Name;
				}
				else
				{
					Params.Add(pair.Value.ToString());
					statement += pair.Name + "=?";
				}

				if (i < updatesSize - 1)
				{
					statement += ",";
				}
			}

			return statement;
		}

		public BaseUpdate Set(Boolean exp, string column, object value)
		{
			if (exp)
			{
				Updates.Add(new Pair(column, value));
			}
			return this;
		}

		public BaseUpdate Set(string column, object value)
		{
			Updates.Add(new Pair(column, value));
			return this;
		}

		public BaseUpdate Set(string setStatement)
		{
			Updates.Add(new StatementPair(setStatement));
			return this;
		}

		public BaseUpdate Set(Boolean exp, string setStatement)
		{
			if (exp)
			{
				Updates.Add(new StatementPair(setStatement));
			}
			return this;
		}

		public BaseUpdate SetIfNotNull(string column, object value)
		{
			return Set(value != null, column, value);
		}

		public BaseUpdate SetIfNotEmpty(string column, object value)
		{
			return Set(!IsEmpty(value), column, value);
		}
	}
}
