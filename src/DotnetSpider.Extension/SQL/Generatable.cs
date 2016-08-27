using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    public abstract class Generatable<T> where T : Generatable<T>
    {

        protected internal string table;

        protected internal string statement;

        protected internal List<string> @params = new List<string>();

        protected internal List<Pair> conditions = new List<Pair>();

        public abstract Command toCommand();

        public string joinNames(List<Pair> pairs)
        {
            if (pairs.Count == 0)
                return "";
            else
            {
                string result = "";
                foreach (Pair pair in pairs)
                {
                    result += pair.name + ",";
                }
                result = result.Substring(0, result.Length - 1);
                return result;
            }
        }

        public string joinValues(List<Pair> pairs)
        {
            if (pairs.Count == 0)
                return "";
            else
            {
                string result = "";
                foreach (Pair pair in pairs)
                {
                    result += pair.value + ",";
                }
                result = result.Substring(0, result.Length - 1);
                return result;
            }
        }


        internal static Boolean isEmpty(object obj)
        {
            if (obj == null)
                return true;

            string str = obj.ToString();
            return str.Length == 0 || str.Trim().Length == 0;
        }

        protected string joinQuestionMarks(List<Pair> pairs)
        {
            StringBuilder s = new StringBuilder();
            for (int size = pairs.Count, i = 0; i < size; i++)
                s.Append('?').Append(i == size - 1 ? "" : ",");
            return s.ToString();
        }

        protected List<string> getValues(List<Pair> pairs)
        {
            if (pairs.Count == 0)
                return new List<string>();

            List<string> result = new List<string>();
            foreach (Pair pair in pairs)
                result.Add(pair.value.ToString());

            return result;
        }

        public T Where(string statement)
        {
            if (this is BaseInsert)
                throw new Exception("cannot use 'where' block in Insert");
            this.conditions.Add(new StatementPair(statement));
            return (T)this;
        }

        public T Where(string column, object value)
        {
            if (this is BaseInsert)
            {
                throw new Exception("cannot use 'where' block in Insert");
            }
            this.conditions.Add(new Pair(column, value));
            return (T)this;
        }

        public T Where(Boolean exp, string statement)
        {
            if (this is BaseInsert)
                throw new Exception("cannot use 'where' block in Insert");
            if (exp)
                this.conditions.Add(new StatementPair(statement));
            return (T)this;
        }

        public T Where(Boolean exp, string column, object value)
        {
            if (this is BaseInsert)
                throw new Exception("cannot use 'where' block in Insert");
            if (exp)
                this.conditions.Add(new Pair(column, value));
            return (T)this;
        }

        public T And(string statement)
        {
            this.conditions.Add(new StatementPair(Pref.AND, statement));
            return (T)this;
        }

        public T And(string column, object value)
        {
            this.conditions.Add(new Pair(Pref.AND, column, value));
            return (T)this;
        }

        public T And(Boolean exp, string statement)
        {
            if (exp)
            {
                this.conditions.Add(new StatementPair(Pref.AND, statement));
            }
            return (T)this;
        }

        public T And(Boolean exp, string column, object value)
        {
            if (exp)
            {
                this.conditions.Add(new Pair(Pref.AND, column, value));
            }
            return (T)this;
        }

        public T AndIfNotEmpty(string column, object value)
        {
            return And(!isEmpty(value), column, value);
        }

        public T Or(string statement)
        {
            this.conditions.Add(new StatementPair(Pref.OR, statement));
            return (T)this;
        }

        public T Or(string column, object value)
        {
            this.conditions.Add(new Pair(Pref.OR, column, value));
            return (T)this;
        }

        public T Or(Boolean exp, string statement)
        {
            if (exp)
            {
                this.conditions.Add(new StatementPair(Pref.OR, statement));
            }
            return (T)this;
        }

        public T Or(Boolean exp, string column, object value)
        {
            if (exp)
                this.conditions.Add(new Pair(Pref.OR, column, value));
            return (T)this;
        }

        public T OrIfNotEmpty(string column, object value)
        {
            return Or(!isEmpty(value), column, value);
        }

        public T Append(string statement)
        {
            this.conditions.Add(new StatementPair(statement));
            return (T)this;
        }

        public T Append(string column, object value)
        {
            this.conditions.Add(new Pair(column, value));
            return (T)this;
        }

        public T Append(Boolean exp, string statement)
        {
            if (exp)
                this.conditions.Add(new StatementPair(statement));
            return (T)this;
        }

        public T Append(Boolean exp, string column, object value)
        {
            if (exp)
                this.conditions.Add(new Pair(column, value));
            return (T)this;
        }

        protected string generateWhereBlock()
        {
            string where = "";

            if (this.conditions.Count > 0)
            {
                where = "WHERE ";

                for (int i = 0, conditionsSize = conditions.Count; i < conditionsSize; i++)
                {
                    Pair condition = conditions[i];
                    where = processCondition(i, where, condition);
                }

            }

            return " " + where;
        }

        private string processCondition(int index, string where, Pair condition)
        {

            where = where.Trim();

            // 第一个条件不能加 and 和 or 前缀
            if (index > 0 && !where.EndsWith("("))
            {
                if (condition.pref == Pref.AND)
                    where += " AND ";
                else if (condition.pref == Pref.OR)
                    where += " OR ";
            }

            where += " ";

            if (condition is StatementPair)
            {       // 不带参数的条件
                where += condition.name;

            }
            else if (condition.value is List<string>)
            {   // 参数为 List 的条件（即 in 条件）
                string marks = "(";

                foreach (string o in (List<string>)condition.value)
                {
                    marks += "?,";
                    this.@params.Add(o);
                }

                if (marks.EndsWith(","))
                {
                    marks = marks.Substring(0, marks.Length - 1);
                }
                marks += ")";                                 // marks = "(?,?,?,...,?)"

                where += condition.name.Replace("?", marks);  // "A in ?" -> "A in (?,?,?)"

            }
            else
            {
                where += condition.name;
                this.@params.Add(condition.value.ToString());
            }

            return where;
        }
    }
}
