using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    public class BaseInsert : Generatable<BaseInsert>
    {
        protected List<Pair> pairs = new List<Pair>();

        public BaseInsert()
        { }

        public BaseInsert(string table)
        {
            this.table = table;
        }

        public BaseInsert Values(string column, object value)
        {
            if (value != null)
                pairs.Add(new Pair(column, value));
            return this;
        }

        public BaseInsert Values(Boolean ifTrue, string column, object value)
        {
            if (ifTrue)
                Values(column, value);
            return this;
        }

        public BaseInsert Values(Dictionary<string, object> map)
        {
            foreach (KeyValuePair<string, object> entry in map.AsEnumerable())
                Values(entry.Key, entry.Value);
            return this;
        }

        public override Command toCommand()
        {
            this.statement = string.Format("INSERT INTO {0}({1}) VALUES ({2})", table, joinNames(pairs), joinQuestionMarks(pairs));
            this.@params = getValues(pairs);

            return new Command(statement, @params);
        }
    }
}
