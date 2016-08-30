namespace DotnetSpider.Extension.Common.Sql
{
	public class Pair
	{
		public Pref Pref { get; set; }

		public string Name { get; set; }

		public object Value { get; set; }

		public Pair(string name, object value)
		{
			Name = name;
			Value = value;
		}

		public Pair(Pref pref, string name, object value)
		{
			Pref = pref;
			Name = name;
			Value = value;
		}
	}

	public class StatementPair : Pair
	{
		public StatementPair(string statement)
			: base(statement, null)
		{

		}

		public StatementPair(Pref pref, string statement)
			: base(pref, statement, null)
		{

		}
	}

	public enum Pref
	{
		And, Or
	}
}

