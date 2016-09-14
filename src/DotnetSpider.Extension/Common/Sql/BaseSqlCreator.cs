namespace DotnetSpider.Extension.Common.Sql
{
	/// <summary>
	/// 生成 Command 的帮助类
	/// </summary>
	public class BaseSqlCreator<TS, TU, TI, TD, TC>
		where TS : BaseSelect, new()
		where TU : BaseUpdate, new()
		where TI : BaseInsert, new()
		where TD : BaseDelete, new()
		where TC : BaseCreate, new()
	{
		public static TS Select(string database, string columns)
		{
			return new TS { Database = database, Columns = columns };
		}

		public static TU Update(string database, string table)
		{
			return new TU { Database = database, Table = table };
		}

		public static TI Insert(string database, string table)
		{
			return new TI { Database = database, Table = table };
		}

		public static TD Delete(string database, string table)
		{
			return new TD { Database = database, Table = table };
		}

		public static TC CreateTable(string database, string table, bool drop)
		{
			return new TC { Database = database, Table = table, DropIfExists = drop };
		}
	}
}

