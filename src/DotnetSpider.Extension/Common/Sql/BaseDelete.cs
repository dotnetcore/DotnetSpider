namespace DotnetSpider.Extension.Common.Sql
{
	public class BaseDelete : Generatable<BaseDelete>
	{
		public BaseDelete() { }

		public BaseDelete(string table)
		{
			Table = table;
		}

		public override Command ToCommand()
		{
			Statement = $"DELETE FROM {Table}{GenerateWhereBlock()}";
			return new Command(Statement, Params);
		}
	}
}
