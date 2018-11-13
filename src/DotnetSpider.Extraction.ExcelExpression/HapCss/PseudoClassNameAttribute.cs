using System;

namespace DotnetSpider.Extraction.ExcelExpression.HapCss
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PseudoClassNameAttribute : Attribute
    {
        public string FunctionName { get; private set; }

        public PseudoClassNameAttribute(string name)
        {
            FunctionName = name;
        }
    }
}
