using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolGood.Algorithm;
using HtmlAgilityPack;
using DotnetSpider.HtmlAgilityPack.Css;

namespace DotnetSpider.Extraction.ExcelExpression
{
	public class ExcelEngine : AlgorithmEngine
	{
		private string _html;
		private List<string> _names = new List<string>();
		public ExcelEngine()
		{
			AddFunction("OuterHtml", F_OuterHtml);
			AddFunction("InnerHtml", F_InnerHtml);
			AddFunction("InnerText", F_InnerText);
			AddFunction("Attr", F_Attr);
			AddFunction("HasClass", F_HasClass);
			AddFunction("HasAttr", F_HasAttr);
		}

		protected override Operand GetParameter(Operand curOpd)
		{
			if (curOpd.Parameter.ToLower() == "[html]") {
				return new Operand(OperandType.STRING, _html);
			}
			return base.GetParameter(curOpd);
		}

		public string TryWork(string name, string formula, string html)
		{
			_html = html;
			if (string.IsNullOrEmpty(name)) {
				return TryEvaluate(formula, null);
			} else {
				if (_names.Contains(name)) {
					var obj = this.Evaluate(name);
					if (obj == null) {
						return null;
					}
					return obj.ToString();

				} else if (Parse(name, formula)) {
					var obj = this.Evaluate(name);
					if (obj == null) {
						return null;
					}
					return obj.ToString();
				}
			}
			throw new InvalidProgramException();
		}

		private Operand F_OuterHtml(List<Operand> ops)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(ops[0].StringValue);
			var useXPath = true;
			if (ops.Count==3) {
				useXPath = ops[2].BooleanValue;
			}
			HtmlNode html;
			if (useXPath) {
				html = document.DocumentNode.SelectSingleNode(ops[1].StringValue);
			} else {
				html = document.QuerySelector(ops[1].StringValue);
			}
			return new Operand(OperandType.STRING, html.OuterHtml);
		}
		private Operand F_InnerHtml(List<Operand> ops)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(ops[0].StringValue);
			var useXPath = true;
			if (ops.Count == 3) {
				useXPath = ops[2].BooleanValue;
			}
			HtmlNode html;
			if (useXPath) {
				html = document.DocumentNode.SelectSingleNode(ops[1].StringValue);
			} else {
				html = document.QuerySelector(ops[1].StringValue);
			}
			return new Operand(OperandType.STRING, html.InnerHtml);
		}
		private Operand F_InnerText(List<Operand> ops)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(ops[0].StringValue);
			var useXPath = true;
			if (ops.Count == 3) {
				useXPath = ops[2].BooleanValue;
			}
			HtmlNode html;
			if (useXPath) {
				html = document.DocumentNode.SelectSingleNode(ops[1].StringValue);
			} else {
				html = document.QuerySelector(ops[1].StringValue);
			}
			return new Operand(OperandType.STRING, html.InnerText);
		}
		private Operand F_Attr(List<Operand> ops)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(ops[0].StringValue);
			var html = document.DocumentNode.FirstChild.Attributes[ops[1].StringValue];
			return new Operand(OperandType.STRING, html.Value);
		}
		private Operand F_HasClass(List<Operand> ops)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(ops[0].StringValue);
			var html = document.DocumentNode.FirstChild.HasClass(ops[1].StringValue);
			return new Operand(OperandType.BOOLEAN, html);
		}
		private Operand F_HasAttr(List<Operand> ops)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(ops[0].StringValue);
			var html = document.DocumentNode.FirstChild.Attributes.Contains(ops[1].StringValue);
			return new Operand(OperandType.BOOLEAN, html);
		}

 


	}
}
