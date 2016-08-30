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
		public static TS Select(string columns)
		{
			return new TS { Columns = columns };
		}

		public static TU Update(string table)
		{
			return new TU { Table = table };
		}

		public static TI Insert(string table)
		{
			return new TI { Table = table };
		}

		public static TD Delete(string table)
		{
			return new TD { Table = table };
		}

		public static TC CreateTable(string table, bool drop)
		{
			return new TC { Table = table, DropIfExists = drop };
		}
	}
}

