using System;
using DotnetSpider.DataFlow.Parser.Formatter;

namespace DotnetSpider.ExcelExpression
{
    /// <summary>
    /// 类 excel 公式提取类,传入值为[html]
    /// 支持OuterHtml、InnerHtml、InnerText、Attr、HasClass、HasAttr
    /// 及 ToolGood.Algorithm 各种类 excel 公式
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExcelFormatter : Formatter
    {
        [ThreadStatic] 
        private static readonly ExcelEngine ExcelEngine;

        static ExcelFormatter()
        {
            ExcelEngine = new ExcelEngine {UseExcelIndex = false};
        }

        public ExcelFormatter(string formula)
        {
            Formula = formula;
        }

        public ExcelFormatter(string formulaName, string formula)
        {
            FormulaName = formulaName;
            Formula = formula;
        }

        public string FormulaName { get; }

        public string Formula { get; }

        protected override string FormatValue(string value)
        {
            if (value != null)
            {
                return ExcelEngine.TryWork(FormulaName, Formula, value);
            }

            return null;
        }

        protected override void CheckArguments()
        {
            if (string.IsNullOrEmpty(Formula))
            {
                throw new ArgumentNullException("Formula 不可为空");
            }
        }
    }
}