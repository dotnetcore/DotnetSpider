using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    public class BaseCreate : Generatable<BaseCreate>
    {
        protected List<Pair> columns = new List<Pair>();
        protected internal bool dropIfExists = false;

        public BaseCreate() { }

        public BaseCreate(string table, bool drop=false)
        {
            this.table = table;
            dropIfExists = drop;
        }

        public override Command toCommand()
        {
            if (dropIfExists)
            {
                this.statement = ($@"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{this.table}') AND type in (N'U'))
DROP TABLE {this.table}
");
            }
            this.statement += $@"CREATE TABLE {table}({generateColumns()}) ON [PRIMARY]";
            return new Command(this.statement, this.@params);
        }

        private string generateColumns()
        {
            string statement = "";

            for (int i = 0, updatesSize = columns.Count; i < updatesSize; i++)
            {
                Pair pair = columns[i];
                if (pair is StatementPair)
                    statement += pair.name;
                else
                {
                    this.@params.Add(pair.value.ToString());
                    statement += $"[{pair.name}] {pair.value.ToString()}";
                }

                if (i < updatesSize - 1)
                {
                    statement += ",";
                }
            }

            return statement;
        }

        public BaseCreate AddColumn(string column, string typeName, string identityString = null, string size = "", bool nullable = true)
        {
            string type = typeName;
            type = string.IsNullOrEmpty(size) ? type : $"{type}({size})";
            type = string.IsNullOrEmpty(identityString) ? type : $"{type} {identityString}";
            type = nullable ? $"{type} NULL" : $"{type} NOT NULL";
            columns.Add(new Pair(column, type));
            return this;
        }
    }
}
