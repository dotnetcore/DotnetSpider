using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL.MSSql
{
    public class Insert : BaseInsert
    {
        public override Command toCommand()
        {
            this.statement = string.Format("INSERT INTO {0}({1}) VALUES ({2})", table, joinNames(pairs), joinValues(pairs));
            this.@params = getValues(pairs);

            return new Command(statement, @params);
        }
    }
}
