using DotnetSpider.Extraction.ExcelExpression;
using DotnetSpider.Extraction.Model.Formatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Extraction
{
	/// <summary>
	/// 类excel 公式提取类,传入值为[html]
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

		protected override object FormateValue(object value)
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
