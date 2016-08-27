using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    /// <summary>
    /// 生成 Command 的帮助类
    /// </summary>
    public class BaseSQL<S, U, I, D, C> 
        where S : BaseSelect, new()
        where U : BaseUpdate, new()
        where I : BaseInsert, new()
        where D : BaseDelete, new()
        where C : BaseCreate, new()
    {
        public static S SELECT(string columns)
        {
            return new S() { columns = columns };
        }

        public static U UPDATE(string table)
        {
            return new U() { table = table };
        }

        public static I INSERT(string table)
        {
            return new I() { table = table };
        }

        public static D DELETE(string table)
        {
            return new D() { table = table };
        }

        public static C CreateTable(string table, bool drop)
        {
            return new C() { table = table, dropIfExists = drop };
        }
    }
}
