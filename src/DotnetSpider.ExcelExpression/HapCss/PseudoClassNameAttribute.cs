using System;

namespace DotnetSpider.ExcelExpression.HapCss
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PseudoClassNameAttribute : Attribute
    {
        public string FunctionName { get; }

        public PseudoClassNameAttribute(string name)
        {
            FunctionName = name;
        }
    }
}
