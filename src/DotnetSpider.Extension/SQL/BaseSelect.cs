using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    /// <summary>
    /// 用于生成 select 语句的帮助类
    /// </summary>
    public class BaseSelect : Generatable<BaseSelect>
    {

        protected internal string columns;

        protected internal string from;

        protected internal string orderBy;

        protected internal string groupBy;

        public BaseSelect()
        { }

        public BaseSelect(string columns)
        {
            this.columns = columns;
        }

        public BaseSelect From(string from)
        {
            this.from = from;
            return this;
        }

        public BaseSelect OrderBy(string orderBy)
        {
            this.orderBy = orderBy;
            return this;
        }

        public BaseSelect GroupBy(string groupBy)
        {
            this.groupBy = groupBy;
            return this;
        }


        public override Command toCommand()
        {
            this.statement = string.Format("SELECT {0} FROM {1} ", this.columns, this.from);

            this.statement += generateWhereBlock();

            if (!isEmpty(this.groupBy))
                this.statement += " GROUP BY " + this.groupBy;

            if (!isEmpty(this.orderBy))
                this.statement += " ORDER BY " + this.orderBy;

            return new Command(this.statement, this.@params);
        }
    }
}
