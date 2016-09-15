namespace DotnetSpider.Extension.Common.Sql
{
	/// <summary>
	/// 用于生成 select 语句的帮助类
	/// </summary>
	public class BaseSelect : Generatable<BaseSelect>
	{
		protected internal string Columns;
		// ReSharper disable once InconsistentNaming
		protected internal string _from;
		// ReSharper disable once InconsistentNaming
		protected internal string _orderBy;
		// ReSharper disable once InconsistentNaming
		protected internal string _groupBy;

		public BaseSelect()
		{ }

		public BaseSelect(string columns)
		{
			Columns = columns;
		}

		public BaseSelect From(string from)
		{
			_from = from;
			return this;
		}

		public BaseSelect OrderBy(string orderBy)
		{
			_orderBy = orderBy;
			return this;
		}

		public BaseSelect GroupBy(string groupBy)
		{
			_groupBy = groupBy;
			return this;
		}


		public override Command ToCommand()
		{
			Statement = $"SELECT {Columns} FROM {_from} ;";

			Statement += GenerateWhereBlock();

			if (!IsEmpty(_groupBy))
			{
				Statement += " GROUP BY " + _groupBy;
			}

			if (!IsEmpty(_orderBy))
			{
				Statement += " ORDER BY " + _orderBy;
			}

			return new Command(Statement, Params);
		}
	}
}

