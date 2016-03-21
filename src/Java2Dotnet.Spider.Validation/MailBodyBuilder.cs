using System.Collections.Generic;
using System.Text;

namespace Java2Dotnet.Spider.Validation
{
	public class MailBodyBuilder
	{
		public string Name { get; }
		public string Corporation { get; }

		private readonly List<ValidateResult> _info = new List<ValidateResult>();

		public MailBodyBuilder(string name, string corporation)
		{
			Name = name;
			Corporation = corporation;
		}

		public MailBodyBuilder AddValidateResult(ValidateResult result)
		{
			_info.Add(result);
			return this;
		}

		public string Build()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(GetHtmlHeader());
			builder.Append("<body>");
			builder.Append($"<div id=\"header\"><h1>Welcome to {Corporation} Workflow</h1><p>This point to the <b>validation</b> report.</p></div><div id=\"main\"><div class=\"section\">");
			builder.Append($"<h2>{Name}</h2>");

			builder.Append("<table width=\"900\">");

			builder.Append(" <tr>");
			foreach (var t in new[] { "编号", "规则", "SQL", "参数", "实际值", "状态", "信息" })
			{
				builder.Append($"<td>{t}</td>");
			}
			builder.Append("</tr>");

			foreach (var i in _info)
			{
				builder.Append(" <tr>");

				builder.Append($"<td>{ _info.IndexOf(i) + 1}</td>");
				builder.Append($"<td>{ i.Description}</td>");
				builder.Append($"<td>{ i.Sql}</td>");
				builder.Append($"<td>{ i.Arguments}</td>");
				builder.Append($"<td>{ i.ActualValue}</td>");

				switch (i.Level)
				{
					case ValidateLevel.Error:
						{
							if (i.IsPass)
							{
								builder.Append($"<td class=\"sucess\">Pass</td>");
							}
							else
							{
								builder.Append($"<td class=\"error\">Error</td>");
							}

							break;
						}
					case ValidateLevel.Warning:
						{
							if (i.IsPass)
							{
								builder.Append($"<td class=\"sucess\">Pass</td>");
							}
							else
							{
								builder.Append($"<td class=\"warning\">Warning</td>");
							}

							break;
						}
					case ValidateLevel.Info:
						{
							builder.Append($"<td>Info</td>");
							break;
						}
				}

				builder.Append($"<td>{ i.Message}</td>");
				builder.Append("</tr>");
			}

			builder.Append("</table><br/>");
			builder.Append("<span>Any question please ask for help from Lewis@86research.com</span></div></div></body></html>");

			return builder.ToString();
		}

		private string GetHtmlHeader()
		{
			return "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>" + Corporation + " Workflow</title><style>html {background: #f1f1f1;height: 80%; width:920px}body {background: #fff;color: #505050;font: 14px 'Segoe UI', tahoma, arial, helvetica, sans-serif;margin: 1%;min-height: 95.5%;border: 1px solid silver;position: relative;}table {border-collapse: collapse;border-spacing: 0;border-left: 1px solid #888;border-top: 1px solid #888;background: #efefef;}th, td {border-right: 1px solid #888;border-bottom: 1px solid #888;padding: 5px 15px;}th {font-weight: bold;background: #ccc;}#header {padding: 0;}#header h1 {font-size: 44px;font-weight: normal;margin: 0;padding: 10px 30px 10px 30px;}#header p {font-size: 20px;color: #fff;background: #007acc;padding: 0 30px;line-height: 50px;margin-top: 25px;}#main {padding: 5px 30px;clear: both;}.section {width: 21.7%;float: left;margin: 0 0 0 4%;}.section h2 {font-size: 13px;text-transform: uppercase;margin: 0;border-bottom: 1px solid silver;padding-bottom: 12px;margin-bottom: 8px;}.section {margin-left: 0;}.section h2 {font-size: 24px;text-transform: none;margin-bottom: 25px;border: none;}.sucess {background: #32cd32;}.error {background: #ff0000;}.warning {background: #ffff00;}</style></head>";
		}
	}
}
