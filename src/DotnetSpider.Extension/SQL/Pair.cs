using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.SQL
{
    public class Pair
    {

        public Pref pref { get; set; }

        public string name { get; set; }

        public object value { get; set; }

        public Pair(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public Pair(Pref pref, string name, object value)
        {
            this.pref = pref;
            this.name = name;
            this.value = value;
        }
    }

    public class StatementPair : Pair
    {

        public StatementPair(string statement)
            : base(statement, null)
        {

        }

        public StatementPair(Pref pref, string statement)
            : base(pref, statement, null)
        {

        }
    }

    public enum Pref
    {
        AND, OR
    }


}
