using DotnetSpider.Extraction.ExcelExpression;
using DotnetSpider.Extraction.Model.Formatter;
using System;

namespace DotnetSpider.Extraction
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
		private static ExcelEngine _excelEngine = new ExcelEngine() { UseExcelIndex = false };

		public ExcelFormatter(string formula)
		{
			Formula = formula;
		}
		public ExcelFormatter(string formulaName, string formula)
		{
			FormulaName = formulaName;
			Formula = formula;
		}

		public string FormulaName { get; private set; }

		public string Formula { get; private set; }

		protected override object FormatValue(object value)
		{
			if (value != null) {
				return _excelEngine.TryWork(FormulaName, Formula, value.ToString());
			}
			return null;
		}

		protected override void CheckArguments()
		{
			if (string.IsNullOrEmpty(Formula)) {
				throw new ArgumentNullException("Formula 不可为空");
			}

		}
	}
}
