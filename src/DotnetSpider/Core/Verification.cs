//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Text;
//using DotnetSpider.Core.Infrastructure;
//using MimeKit;
//using System.Linq;
//using System.Web;
//using System.IO;
//using Serilog;
//
//namespace DotnetSpider.Extension.Infrastructure
//{
//	/// <summary>
//	/// 查询数据库结果
//	/// </summary>
//	public class QueryResult
//	{
//		/// <summary>
//		/// 查询数据库结果
//		/// </summary>
//		public dynamic Result;
//	}
//
//	/// <summary>
//	/// 验证结果
//	/// </summary>
//	public class VerificationInfo
//	{
//		/// <summary>
//		/// 是否验证通过
//		/// </summary>
//		public bool Pass { get; set; }
//
//		/// <summary>
//		/// 验证结果
//		/// </summary>
//		public string Report { get; set; }
//	}
//
//	/// <summary>
//	/// 验证接口
//	/// </summary>
//	public interface IVerification
//	{
//		/// <summary>
//		/// 名称
//		/// </summary>
//		string Name { get; }
//
//		/// <summary>
//		/// 验证规则命名
//		/// </summary>
//		string VerificationName { get; }
//
//		/// <summary>
//		/// 执行验证操作
//		/// </summary>
//		/// <param name="conn">数据库连接</param>
//		/// <returns></returns>
//		VerificationInfo Verify(IDbConnection conn);
//	}
//
//	/// <summary>
//	/// 验证接口的抽象
//	/// </summary>
//	public abstract class BaseVerification
//	{
//		/// <summary>
//		/// 所有验证规则
//		/// </summary>
//		protected List<IVerification> Verifications { get; set; } = new List<IVerification>();
//
//		/// <summary>
//		/// 邮件接收人
//		/// </summary>
//		public List<string> EmailTo { get; set; }
//
//		/// <summary>
//		/// 邮件发送服务地址
//		/// </summary>
//		public string EmailHost { get; set; }
//
//		/// <summary>
//		/// 邮件的标题
//		/// </summary>
//		public string Subject { get; set; }
//
//		/// <summary>
//		/// 爬虫任务的描述
//		/// </summary>
//		public string Description;
//
//		/// <summary>
//		/// 邮件发送服务端口
//		/// </summary>
//		public int EmailPort { get; set; } = 25;
//
//		/// <summary>
//		/// 邮件发送服务的用户名
//		/// </summary>
//		public string EmailAccount { get; set; }
//
//		/// <summary>
//		/// 邮件发送服务的密码
//		/// </summary>
//		public string EmailPassword { get; set; }
//
//		/// <summary>
//		/// 邮件发送服务的显示名称
//		/// </summary>
//		public string EmailDisplayName { get; set; }
//
//		/// <summary>
//		/// 构造方法
//		/// </summary>
//		protected BaseVerification()
//		{
//			EmailHost = Core.Env.EmailHost;
//			EmailAccount = Core.Env.EmailAccount;
//			EmailPassword = Core.Env.EmailPassword;
//			EmailDisplayName = Core.Env.EmailDisplayName;
//			var portStr = Core.Env.EmailPort;
//			if (!string.IsNullOrWhiteSpace(portStr))
//			{
//				if (int.TryParse(portStr, out var port))
//				{
//					EmailPort = port;
//				}
//				else
//				{
//					Log.Logger.Error($"EmailPort is not a number: {portStr}.");
//				}
//			}
//		}
//
//		/// <summary>
//		/// 构造方法
//		/// </summary>
//		/// <param name="emailTo">邮件接收人</param>
//		/// <param name="subject">邮件的标题</param>
//		/// <param name="host">邮件发送服务地址</param>
//		/// <param name="port">邮件发送服务端口</param>
//		/// <param name="account">邮件发送服务的用户名</param>
//		/// <param name="password">邮件发送服务的密码</param>
//		protected BaseVerification(string emailTo, string subject, string host, int port, string account,
//			string password)
//		{
//			EmailTo = emailTo.Split(';').Select(e => e.Trim()).ToList();
//			EmailHost = host;
//			EmailPort = port;
//			EmailAccount = account;
//			EmailPassword = password;
//			Subject = subject;
//		}
//
//		/// <summary>
//		/// 添加相等的判断
//		/// </summary>
//		/// <param name="name">规则名称</param>
//		/// <param name="sql">SQL语句, 必须包含Result结果, 如: SELECT COUNT(*) AS Result FROM db.table1</param>
//		/// <param name="value">期望等于的值</param>
//		public void AddSqlEqual(string name, string sql, dynamic value)
//		{
//			Verifications.Add(new SqlEqual(name, sql, value));
//		}
//
//		/// <summary>
//		/// 添加大于的判断
//		/// </summary>
//		/// <param name="name">规则名称</param>
//		/// <param name="sql">SQL语句, 必须包含Result结果, 如: SELECT COUNT(*) AS Result FROM db.table1</param>
//		/// <param name="value">期望大于的值</param>
//		public void AddSqlLarge(string name, string sql, dynamic value)
//		{
//			Verifications.Add(new SqlLarge(name, sql, value));
//		}
//
//		/// <summary>
//		/// 添加小于的判断
//		/// </summary>
//		/// <param name="name">规则名称</param>
//		/// <param name="sql">SQL语句, 必须包含Result结果, 如: SELECT COUNT(*) AS Result FROM db.table1</param>
//		/// <param name="value">期望小于的值</param>
//		public void AddSqlLess(string name, string sql, dynamic value)
//		{
//			Verifications.Add(new SqlLess(name, sql, value));
//		}
//
//		/// <summary>
//		/// 添加范围的判断
//		/// </summary>
//		/// <param name="name">规则名称</param>
//		/// <param name="sql">SQL语句, 必须包含Result结果, 如: SELECT COUNT(*) AS Result FROM db.table1</param>
//		/// <param name="minValue">期望的最小值</param>
//		/// <param name="maxValue">期望的最大值</param>
//		public void AddSqlRange(string name, string sql, dynamic minValue, dynamic maxValue)
//		{
//			Verifications.Add(new SqlRange(name, sql, minValue, maxValue));
//		}
//
//		/// <summary>
//		/// 添加相等的判断, 用于如数据存在内存中
//		/// </summary>
//		/// <param name="name">规则名称</param>
//		/// <param name="actual">真实值</param>
//		/// <param name="expected">期望值</param>
//		public void AddValueEqual(string name, dynamic actual, dynamic expected)
//		{
//			Verifications.Add(new ValueEqual(name, actual, expected));
//		}
//
//		/// <summary>
//		/// 添加大于的判断, 用于如数据存在内存中
//		/// </summary>
//		/// <param name="name">规则名称</param>
//		/// <param name="actual">真实值</param>
//		/// <param name="value">期望大于的值</param>
//		public void AddValueLarge(string name, dynamic actual, dynamic value)
//		{
//			Verifications.Add(new ValueLarge(name, actual, value));
//		}
//
//		/// <summary>
//		/// 添加小于的判断, 用于如数据存在内存中
//		/// </summary>
//		/// <param name="name">规则名称</param>
//		/// <param name="actual">真实值</param>
//		/// <param name="value">期望小于的值</param>
//		public void AddValueLess(string name, dynamic actual, dynamic value)
//		{
//			Verifications.Add(new ValueLess(name, actual, value));
//		}
//
//		/// <summary>
//		/// 添加范围的判断, 用于如数据存在内存中
//		/// </summary>
//		/// <param name="name">规则名称</param>
//		/// <param name="actual">真实值</param>
//		/// <param name="minValue">期望的最小值</param>
//		/// <param name="maxValue">期望的最大值</param>
//		public void AddValueRange(string name, dynamic actual, dynamic minValue, dynamic maxValue)
//		{
//			Verifications.Add(new ValueRange(name, actual, minValue, maxValue));
//		}
//
//		/// <summary>
//		/// 验证并发送报告
//		/// </summary>
//		/// <returns>验证的最终结果</returns>
//		public abstract VerificationResult Report();
//
//		abstract class BaseSqlVerification : IVerification
//		{
//			public string Name { get; protected set; }
//			public string VerificationName { get; protected set; }
//			public string Sql { get; protected set; }
//			protected dynamic[] Values { get; set; }
//
//			public VerificationInfo Verify(IDbConnection conn)
//			{
//				bool verifyResult;
//				Object result;
//				string color;
//				string verifyResultStr;
//
//				try
//				{
//					var query = conn.MyQuery<QueryResult>(Sql).FirstOrDefault();
//					result = query?.Result;
//					verifyResult = Validate(result);
//					color = verifyResult ? "forestgreen" : "orangered";
//					verifyResultStr = verifyResult ? "PASS" : "FAILED";
//				}
//				catch (Exception e)
//				{
//					result = e.ToString();
//					verifyResult = false;
//					color = "orangered";
//					verifyResultStr = "FAILED";
//				}
//
//				var report =
//					"<tr>" +
//					$"<td>{Name}</td>" +
//					$"<td>{VerificationName}</td>" +
//					$"<td>{Sql}</td>" +
//					$"<td>{ExpectedValue}</td>" +
//					$"<td>{result}</td>" +
//					$"<td style=\"color:{color}\"><strong>{verifyResultStr}</strong></td>" +
//					$"<td>{DateTime.Now:yyyy-MM-dd hh:mm:ss}</td>" +
//					"</tr>";
//
//				return new VerificationInfo
//				{
//					Pass = verifyResult,
//					Report = report
//				};
//			}
//
//			public abstract dynamic ExpectedValue { get; }
//
//			public abstract bool Validate(dynamic result);
//		}
//
//		class SqlEqual : BaseSqlVerification
//		{
//			public SqlEqual(string name, string sql, dynamic value)
//			{
//				Sql = sql;
//				Values = new[] {value};
//				Name = name;
//				VerificationName = "SQLEqual";
//			}
//
//			public override dynamic ExpectedValue => Values[0];
//
//			public override bool Validate(dynamic result)
//			{
//				return result == ExpectedValue;
//			}
//		}
//
//		class SqlLarge : BaseSqlVerification
//		{
//			public SqlLarge(string name, string sql, dynamic value)
//			{
//				Name = name;
//				Sql = sql;
//				VerificationName = "SQLLarge";
//				Values = new[] {value};
//			}
//
//			public override dynamic ExpectedValue => Values[0];
//
//			public override bool Validate(dynamic result)
//			{
//				return result > ExpectedValue;
//			}
//		}
//
//		class SqlLess : BaseSqlVerification
//		{
//			public SqlLess(string name, string sql, dynamic value)
//			{
//				Name = name;
//				Sql = sql;
//				VerificationName = "SQLLess";
//				Values = new[] {value};
//			}
//
//			public override dynamic ExpectedValue => Values[0];
//
//			public override bool Validate(dynamic result)
//			{
//				return result < ExpectedValue;
//			}
//		}
//
//		class SqlRange : BaseSqlVerification
//		{
//			public SqlRange(string name, string sql, dynamic minValue, dynamic maxValue)
//			{
//				Name = name;
//				Sql = sql;
//				VerificationName = "SQLRange";
//				Values = new[] {minValue, maxValue};
//			}
//
//			public override dynamic ExpectedValue => $"{Values[0]}-{Values[1]}";
//
//			public override bool Validate(dynamic result)
//			{
//				return result >= Values[0] && result <= Values[1];
//			}
//		}
//
//		abstract class BaseValueVerification : IVerification
//		{
//			public string Name { get; protected set; }
//			public string VerificationName { get; protected set; }
//			public dynamic Acutal { get; protected set; }
//			protected dynamic[] Expected { get; set; }
//
//			public VerificationInfo Verify(IDbConnection conn)
//			{
//				bool verifyResult;
//				string color;
//				string verifyResultStr;
//
//				try
//				{
//					verifyResult = Validate(Acutal);
//					color = verifyResult ? "forestgreen" : "orangered";
//					verifyResultStr = verifyResult ? "PASS" : "FAILED";
//				}
//				catch (Exception e)
//				{
//					Acutal = e.ToString();
//					verifyResult = false;
//					color = "orangered";
//					verifyResultStr = "FAILED";
//				}
//
//				var report =
//					"<tr>" +
//					$"<td>{Name}</td>" +
//					$"<td>{VerificationName}</td>" +
//					"<td>NONE</td>" +
//					$"<td>{ExpectedValue}</td>" +
//					$"<td>{Acutal}</td>" +
//					$"<td style=\"color:{color}\"><strong>{verifyResultStr}</strong></td>" +
//					$"<td>{DateTime.Now:yyyy-MM-dd hh:mm:ss}</td>" +
//					"</tr>";
//
//				return new VerificationInfo
//				{
//					Pass = verifyResult,
//					Report = report
//				};
//			}
//
//			public abstract dynamic ExpectedValue { get; }
//
//			public abstract bool Validate(dynamic result);
//		}
//
//		class ValueEqual : BaseValueVerification
//		{
//			public ValueEqual(string name, dynamic actual, dynamic expected)
//			{
//				Expected = new[] {expected};
//				Name = name;
//				Acutal = actual;
//				VerificationName = "ValueEqual";
//			}
//
//			public override dynamic ExpectedValue => Expected[0];
//
//			public override bool Validate(dynamic result)
//			{
//				return result == ExpectedValue;
//			}
//		}
//
//		class ValueLarge : BaseValueVerification
//		{
//			public ValueLarge(string name, dynamic actual, dynamic expected)
//			{
//				Name = name;
//				Acutal = actual;
//				VerificationName = "ValueLarge";
//				Expected = new[] {expected};
//			}
//
//			public override dynamic ExpectedValue => Expected[0];
//
//			public override bool Validate(dynamic result)
//			{
//				return result > ExpectedValue;
//			}
//		}
//
//		class ValueLess : BaseValueVerification
//		{
//			public ValueLess(string name, dynamic actual, dynamic expected)
//			{
//				Name = name;
//				Acutal = actual;
//				VerificationName = "ValueLess";
//				Expected = new[] {expected};
//			}
//
//			public override dynamic ExpectedValue => Expected[0];
//
//			public override bool Validate(dynamic result)
//			{
//				return result < ExpectedValue;
//			}
//		}
//
//		class ValueRange : BaseValueVerification
//		{
//			public ValueRange(string name, dynamic actual, dynamic minValue, dynamic maxValue)
//			{
//				Name = name;
//				Acutal = actual;
//				VerificationName = "ValueRange";
//				Expected = new[] {minValue, maxValue};
//			}
//
//			public override dynamic ExpectedValue => $"{Expected[0]}-{Expected[1]}";
//
//			public override bool Validate(dynamic result)
//			{
//				return result >= Expected[0] && result <= Expected[1];
//			}
//		}
//	}
//
//	/// <summary>
//	/// 验证的最终结果
//	/// </summary>
//	public class VerificationResult
//	{
//		/// <summary>
//		/// 是否通过所有验证规则的检测
//		/// </summary>
//		public bool Success { get; set; }
//	}
//
//	/// <summary>
//	/// 验证类, 验证报告中可以在内容中插入一段样列数据
//	/// </summary>
//	public class Verification : BaseVerification
//	{
//		/// <summary>
//		/// 验证报告中可以在内容中插入一段样列数据
//		/// </summary>
//		private readonly string _reportSampleSql;
//
//		/// <summary>
//		/// 验证报告附件的数据
//		/// </summary>
//		private readonly string _exportDataSql;
//
//		/// <summary>
//		/// 验证报告数据的名件名
//		/// </summary>
//		private readonly string _exportDataFileName;
//
//		/// <summary>
//		/// 构造方法, 验证报告中可以在内容中插入一段样列数据
//		/// </summary>
//		/// <param name="emailTo"></param>
//		/// <param name="reportSampleSql">样例数据的查询语句</param>
//		/// <param name="dataSql">验证报告附件的数据查询SQL语句</param>
//		/// <param name="dataFileName">附件的数据的文件名</param>
//		/// <param name="subject"></param>
//		public Verification(
//			string subject,
//			string emailTo,
//			string reportSampleSql = null,
//			string dataSql = null,
//			string dataFileName = null
//		)
//		{
//			EmailTo = emailTo?.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim())
//				.ToList();
//			Subject = subject;
//			_reportSampleSql = reportSampleSql;
//			_exportDataSql = dataSql;
//			_exportDataFileName = dataFileName;
//		}
//
//		/// <summary>
//		/// 构造方法
//		/// </summary>
//		/// <param name="type">爬虫任务的类型</param>
//		/// <param name="emailTo">邮件接收人</param>
//		/// <param name="subject">邮件的标题</param>
//		/// <param name="host">邮件发送服务地址</param>
//		/// <param name="port">邮件发送服务端口</param>
//		/// <param name="account">邮件发送服务的用户名</param>
//		/// <param name="password">邮件发送服务的密码</param>
//		/// <param name="reportSampleSql">样例数据的查询语句</param>
//		/// <param name="dataSql">验证报告附件的数据查询SQL语句</param>
//		/// <param name="dataFileName">附件的数据的文件名</param>
//		public Verification(string emailTo, string subject, string host, int port, string account,
//			string password,
//			string reportSampleSql = null, string dataSql = null, string dataFileName = null) : base(emailTo, subject,
//			host,
//			port, account, password)
//		{
//			_exportDataSql = dataSql;
//			_exportDataFileName = dataFileName;
//			_reportSampleSql = reportSampleSql;
//		}
//
//		/// <summary>
//		/// 验证并发送报告
//		/// </summary>
//		/// <returns>验证的最终结果</returns>
//		public override VerificationResult Report()
//		{
//			VerificationResult veridationResult = new VerificationResult();
//			if (Core.Env.DataConnectionStringSettings == null)
//			{
//				return veridationResult;
//			}
//
//			if (!string.IsNullOrWhiteSpace(_reportSampleSql) && _reportSampleSql.ToLower().Contains("limit"))
//			{
//				Log.Logger.Error("SQL contains 'LIMIT'.");
//				return veridationResult;
//			}
//
//			if (Verifications != null && Verifications.Count > 0 && EmailTo != null && EmailTo.Count > 0 &&
//			    !string.IsNullOrWhiteSpace(EmailHost))
//			{
//				using (var conn = Core.Env.DataConnectionStringSettings.CreateDbConnection())
//				{
//					var emailBody = new StringBuilder();
//					var hasProperties = _description != null;
//					emailBody.Append(
//						"<html><head>" +
//						"<meta charset=\"utf-8\">" +
//						"<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
//						"<meta name=\"viewport\" content=\"width=device-width initial-scale=1.0\">" +
//						$"<title>{Subject}: {DateTime.Now}</title>" +
//						"<style>" +
//						"table {border-collapse: collapse;border-spacing: 0;border-left: 1px solid #888;border-top: 1px solid #888;background: #efefef;}th, td {border-right: 1px solid #888;border-bottom: 1px solid #888;padding: 5px 15px;}th {font-weight: bold;background: #ccc;}" +
//						"</style>" +
//						"</head>" +
//						"<body style=\"background-color:#FAF7EC\">" +
//						$"<h2>{Subject}: {DateTime.Now}</h2>" +
//						(hasProperties ? $"<strong>Analyst: </strong>{_description.Owner}" : "") +
//						(hasProperties
//							? $"&nbsp;&nbsp;&nbsp;<strong>Developer: </strong>{_description.Developer}"
//							: "") +
//						(hasProperties ? $"&nbsp;&nbsp;&nbsp;<strong>Date: </strong>{_description.Date}" : "") +
//						(hasProperties ? $"&nbsp;&nbsp;&nbsp;<strong>Description: </strong>{Description}" : "") +
//						"<br/><br/>" +
//						"<table>" +
//						"<thead>" +
//						"<tr>" +
//						"<th>Item</th>" +
//						"<th>Rule</th>" +
//						"<th>SQL</th>" +
//						"<th>Expected</th>" +
//						"<th>Actual</th>" +
//						"<th>Result</th>" +
//						"<th>Time</th> " +
//						"</tr>" +
//						"</thead>" +
//						"<tbody>"
//					);
//					var success = true;
//					foreach (var verifier in Verifications)
//					{
//						var result = verifier.Verify(conn);
//						emailBody.AppendLine(result.Report);
//						if (success && !result.Pass)
//						{
//							success = false;
//						}
//					}
//
//					veridationResult.Success = success;
//					emailBody.Append("</tbody></table><br/>");
//					if (!string.IsNullOrWhiteSpace(_reportSampleSql))
//					{
//						emailBody.Append("<strong>数据样本</strong><br/><br/>");
//						emailBody.Append(conn.ToHtml($"{_reportSampleSql} LIMIT 100;"));
//					}
//
//					emailBody.Append("<br/><br/></body></html>");
//
//					var message = new MimeMessage();
//					var displayName = string.IsNullOrWhiteSpace(EmailDisplayName)
//						? "DotnetSpider Alert"
//						: EmailDisplayName;
//					message.From.Add(new MailboxAddress(displayName, EmailAccount));
//					foreach (var emailTo in EmailTo)
//					{
//						message.To.Add(new MailboxAddress(emailTo, emailTo));
//					}
//
//					message.Subject = $"{Subject}: {(success ? "Success" : "Failed")}";
//
//					var html = new TextPart("html")
//					{
//#if NETFRAMEWORK
//						Text = System.Net.WebUtility.HtmlDecode(emailBody.ToString())
//#else
//						Text = HttpUtility.HtmlDecode(emailBody.ToString())
//#endif
//					};
//					var multipart = new Multipart("mixed") {html};
//
//					if (veridationResult.Success && !string.IsNullOrWhiteSpace(_exportDataSql) &&
//					    !string.IsNullOrWhiteSpace(_exportDataFileName))
//					{
//						var path = conn.Export(_exportDataSql, $"{_exportDataFileName}_{DateTime.Now:yyyyMMddhhmmss}",
//							true);
//						var attachment = new MimePart("excel", "xlsx")
//						{
//#if !NET40
//							Content = new MimeContent(File.OpenRead(path)),
//#else
//							ContentObject = new ContentObject(File.OpenRead(path)),
//#endif
//							ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
//							ContentTransferEncoding = ContentEncoding.Base64,
//							FileName = Path.GetFileName(path)
//						};
//						multipart.Add(attachment);
//					}
//
//					message.Body = multipart;
//
//					using (var client = new MailKit.Net.Smtp.SmtpClient())
//					{
//						client.Connect(EmailHost, EmailPort);
//
//						// Note: only needed if the SMTP server requires authentication
//						client.Authenticate(EmailAccount, EmailPassword);
//
//						client.Send(message);
//						client.Disconnect(true);
//					}
//				}
//			}
//
//
//			return veridationResult;
//		}
//	}
//
//	/// <summary>
//	/// 验证类, 验证报告中可以在内容中插入一段样列数据
//	/// </summary>
//	/// <typeparam name="TE">爬虫任务的类型</typeparam>
//	public class Verification<TE> : Verification
//	{
//		/// <summary>
//		/// 构造方法
//		/// </summary>
//		/// <param name="reportSampleSql">样例数据的查询语句</param>
//		/// <param name="dataSql">附件的数据查询SQL语句</param>
//		/// <param name="dataFileName">附件的数据的文件名</param>
//		public Verification(string reportSampleSql = null, string dataSql = null, string dataFileName = null) : base(
//			typeof(TE), reportSampleSql, dataSql, dataFileName)
//		{
//		}
//
//		/// <summary>
//		/// 构造方法
//		/// </summary>
//		/// <param name="emailTo">邮件接收人</param>
//		/// <param name="subject">邮件的标题</param>
//		/// <param name="host">邮件发送服务地址</param>
//		/// <param name="port">邮件发送服务端口</param>
//		/// <param name="account">邮件发送服务的用户名</param>
//		/// <param name="password">邮件发送服务的密码</param>
//		/// <param name="reportSampleSql">样例数据的查询语句</param>
//		/// <param name="dataSql">附件的数据查询SQL语句</param>
//		/// <param name="dataFileName">附件的数据的文件名</param>
//		public Verification(string emailTo, string subject, string host, int port, string account, string password,
//			string reportSampleSql = null, string dataSql = null, string dataFileName = null) : base(typeof(TE),
//			emailTo,
//			subject, host, port, account, password, reportSampleSql, dataSql, dataFileName)
//		{
//		}
//	}
//}