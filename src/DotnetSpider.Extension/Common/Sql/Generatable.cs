using System;
using System.Collections.Generic;
using System.Text;
using DotnetSpider.Core;

namespace DotnetSpider.Extension.Common.Sql
{
	public abstract class Generatable<T> where T : Generatable<T>
	{
		protected internal string Table;

		protected internal string Statement;

		protected internal List<string> Params = new List<string>();

		protected internal List<Pair> Conditions = new List<Pair>();

		public abstract Command ToCommand();

		public string JoinNames(List<Pair> pairs)
		{
			if (pairs.Count == 0)
			{
				return "";
			}
			else
			{
				string result = "";
				foreach (Pair pair in pairs)
				{
					result += pair.Name + ",";
				}
				result = result.Substring(0, result.Length - 1);
				return result;
			}
		}

		public string JoinValues(List<Pair> pairs)
		{
			if (pairs.Count == 0)
			{
				return "";
			}
			else
			{
				string result = "";
				foreach (Pair pair in pairs)
				{
					result += pair.Value + ",";
				}
				result = result.Substring(0, result.Length - 1);
				return result;
			}
		}


		internal static Boolean IsEmpty(object obj)
		{
			if (obj == null)
			{
				return true;
			}
			string str = obj.ToString();
			return str.Length == 0 || str.Trim().Length == 0;
		}

		protected string JoinQuestionMarks(List<Pair> pairs)
		{
			StringBuilder s = new StringBuilder();
			for (int size = pairs.Count, i = 0; i < size; i++)
			{
				s.Append('?').Append(i == size - 1 ? "" : ",");
			}

			return s.ToString();
		}

		protected List<string> GetValues(List<Pair> pairs)
		{
			if (pairs.Count == 0)
			{
				return new List<string>();
			}
			List<string> result = new List<string>();
			foreach (Pair pair in pairs)
			{
				result.Add(pair.Value.ToString());
			}

			return result;
		}

		public T Where(string statement)
		{
			if (this is BaseInsert)
			{
				throw new SpiderException("cannot use 'where' block in Insert");
			}
			Conditions.Add(new StatementPair(statement));
			return (T)this;
		}

		public T Where(string column, object value)
		{
			if (this is BaseInsert)
			{
				throw new SpiderException("cannot use 'where' block in Insert");
			}
			Conditions.Add(new Pair(column, value));
			return (T)this;
		}

		public T Where(Boolean exp, string statement)
		{
			if (this is BaseInsert)
			{
				throw new SpiderException("cannot use 'where' block in Insert");
			}
			if (exp)
			{
				Conditions.Add(new StatementPair(statement));
			}
			return (T)this;
		}

		public T Where(Boolean exp, string column, object value)
		{
			if (this is BaseInsert)
			{
				throw new SpiderException("cannot use 'where' block in Insert");
			}
			if (exp)
			{
				Conditions.Add(new Pair(column, value));
			}
			return (T)this;
		}

		public T And(string statement)
		{
			Conditions.Add(new StatementPair(Pref.And, statement));
			return (T)this;
		}

		public T And(string column, object value)
		{
			Conditions.Add(new Pair(Pref.And, column, value));
			return (T)this;
		}

		public T And(Boolean exp, string statement)
		{
			if (exp)
			{
				Conditions.Add(new StatementPair(Pref.And, statement));
			}
			return (T)this;
		}

		public T And(Boolean exp, string column, object value)
		{
			if (exp)
			{
				Conditions.Add(new Pair(Pref.And, column, value));
			}
			return (T)this;
		}

		public T AndIfNotEmpty(string column, object value)
		{
			return And(!IsEmpty(value), column, value);
		}

		public T Or(string statement)
		{
			Conditions.Add(new StatementPair(Pref.Or, statement));
			return (T)this;
		}

		public T Or(string column, object value)
		{
			Conditions.Add(new Pair(Pref.Or, column, value));
			return (T)this;
		}

		public T Or(Boolean exp, string statement)
		{
			if (exp)
			{
				Conditions.Add(new StatementPair(Pref.Or, statement));
			}
			return (T)this;
		}

		public T Or(Boolean exp, string column, object value)
		{
			if (exp)
			{
				Conditions.Add(new Pair(Pref.Or, column, value));
			}
			return (T)this;
		}

		public T OrIfNotEmpty(string column, object value)
		{
			return Or(!IsEmpty(value), column, value);
		}

		public T Append(string statement)
		{
			Conditions.Add(new StatementPair(statement));
			return (T)this;
		}

		public T Append(string column, object value)
		{
			Conditions.Add(new Pair(column, value));
			return (T)this;
		}

		public T Append(Boolean exp, string statement)
		{
			if (exp)
			{
				Conditions.Add(new StatementPair(statement));
			}
			return (T)this;
		}

		public T Append(Boolean exp, string column, object value)
		{
			if (exp)
			{
				Conditions.Add(new Pair(column, value));
			}
			return (T)this;
		}

		protected string GenerateWhereBlock()
		{
			string where = "";

			if (Conditions.Count > 0)
			{
				where = "WHERE ";

				for (int i = 0, conditionsSize = Conditions.Count; i < conditionsSize; i++)
				{
					Pair condition = Conditions[i];
					where = ProcessCondition(i, where, condition);
				}
			}

			return " " + where;
		}

		private string ProcessCondition(int index, string where, Pair condition)
		{
			where = where.Trim();

			// 第一个条件不能加 and 和 or 前缀
			if (index > 0 && !where.EndsWith("("))
			{
				if (condition.Pref == Pref.And)
				{
					where += " AND ";
				}
				else if (condition.Pref == Pref.Or)
				{
					where += " OR ";
				}
			}

			where += " ";

			if (condition is StatementPair)
			{
				// 不带参数的条件
				where += condition.Name;
			}
			else if (condition.Value is List<string>)
			{
				// 参数为 List 的条件（即 in 条件）
				string marks = "(";

				foreach (string o in (List<string>)condition.Value)
				{
					marks += "?,";
					Params.Add(o);
				}

				if (marks.EndsWith(","))
				{
					marks = marks.Substring(0, marks.Length - 1);
				}
				marks += ")";                                 // marks = "(?,?,?,...,?)"

				where += condition.Name.Replace("?", marks);  // "A in ?" -> "A in (?,?,?)"
			}
			else
			{
				where += condition.Name;
				Params.Add(condition.Value.ToString());
			}

			return where;
		}
	}
}
