using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    /**
     * 用于生成 update 语句的帮助类
     */
    public class BaseUpdate : Generatable<BaseUpdate>
    {

        protected List<Pair> updates = new List<Pair>();

        public BaseUpdate() { }

        public BaseUpdate(string table)
        {
            this.table = table;
        }

        public override Command toCommand()
        {
            this.statement = string.Format("UPDATE {0} set {1} {2}", table, generateSetBlock(), generateWhereBlock());

            return new Command(this.statement, this.@params);
        }

        private string generateSetBlock()
        {
            string statement = "";

            for (int i = 0, updatesSize = updates.Count; i < updatesSize; i++)
            {
                Pair pair = updates[i];
                if (pair is StatementPair)
                    statement += pair.name;
                else
                {
                    this.@params.Add(pair.value.ToString());
                    statement += pair.name + "=?";
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
                this.updates.Add(new Pair(column, value));
            }
            return this;
        }

        public BaseUpdate Set(string column, object value)
        {
            this.updates.Add(new Pair(column, value));
            return this;
        }

        public BaseUpdate Set(string setStatement)
        {
            this.updates.Add(new StatementPair(setStatement));
            return this;
        }

        public BaseUpdate Set(Boolean exp, string setStatement)
        {
            if (exp)
                this.updates.Add(new StatementPair(setStatement));
            return this;
        }

        public BaseUpdate SetIfNotNull(string column, object value)
        {
            return Set(value != null, column, value);
        }

        public BaseUpdate SetIfNotEmpty(string column, object value)
        {
            return Set(!isEmpty(value), column, value);
        }
    }
}
