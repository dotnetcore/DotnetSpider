namespace DotnetSpider.Extension.Common.Sql.MSSql
{
	public class Insert : BaseInsert
	{
		public override Command ToCommand()
		{
			Statement = $"INSERT INTO {Table}({JoinNames(Pairs)}) VALUES ({JoinValues(Pairs)})";
			Params = GetValues(Pairs);

			return new Command(Statement, Params);
		}
	}
}
