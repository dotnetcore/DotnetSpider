using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    public class BaseDelete : Generatable<BaseDelete>
    {
        public BaseDelete() { }

        public BaseDelete(string table)
        {
            this.table = table;
        }

        public override Command toCommand()
        {
            this.statement = string.Format("DELETE FROM {0}{1}", table, generateWhereBlock());
            return new Command(this.statement, this.@params);
        }
    }
}
