using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    /// <summary>
    /// 表示 SQL 命令的类。其中包含 SQL 语句和参数两个部分。参数的值要和 SQL 语句中的问号一一对应。 
    /// </summary>
    public class Command
    {
        private string statement;

        private List<string> @params;

        private List<string> paramTypes;

        /// <summary>
        /// 缺省构造函数
        /// </summary>
        public Command()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="statement">SQL 语句</param>
        /// <param name="params">参数</param>
        public Command(string statement, List<string> @params)
        {
            this.statement = statement;
            this.@params = @params;
        }

        public Command(string statement, List<string> @params, List<string> paramTypes)
        {
            this.statement = statement;
            this.@params = @params;
            this.paramTypes = paramTypes;
        }

        public List<string> getParamTypes()
        {
            return paramTypes;
        }

        public void setParamTypes(List<string> paramTypes)
        {
            this.paramTypes = paramTypes;
        }

        /// <summary>
        /// 获得 SQL 语句
        /// </summary>
        /// <returns>SQL 语句</returns>
        public string getStatement()
        {
            return statement;
        }

        /// <summary>
        /// 设置 SQL 语句
        /// </summary>
        /// <param name="statement">SQL 语句</param>
        public void setStatement(string statement)
        {
            this.statement = statement;
        }

        /// <summary>
        /// 获得参数
        /// </summary>
        /// <returns>参数</returns>
        public List<string> getParams()
        {
            return @params;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="params">参数</param>
        public void setParams(List<string> @params)
        {
            this.@params = @params;
        }

        public override string ToString()
        {
            return "Command{" +
                    "statement='" + statement + '\'' +
                    ", params=" + @params +
                    '}';
        }
    }
}
