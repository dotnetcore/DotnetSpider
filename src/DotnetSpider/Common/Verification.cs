using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using MailKit.Net.Smtp;
using MimeKit;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 验证结果
	/// </summary>
	public class VerificationMessage
	{
		/// <summary>
		/// 是否验证通过
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// 验证结果
		/// </summary>
		public string Message { get; set; }
	}

	/// <summary>
	/// 验证接口
	/// </summary>
	public interface IVerification
	{
		/// <summary>
		/// 名称
		/// </summary>
		string Name { get; }

		/// <summary>
		/// 验证规则命名
		/// </summary>
		string VerificationName { get; }

		/// <summary>
		/// 执行验证操作
		/// </summary>
		/// <returns></returns>
		Task<VerificationMessage> VerifyAsync(params dynamic[] args);
	}

	/// <summary>
	/// 验证接口的抽象
	/// </summary>
	public abstract class BaseVerification
	{
		protected readonly SpiderOptions Options;

		/// <summary>
		/// 所有验证规则
		/// </summary>
		protected List<IVerification> Verifications { get; set; } = new List<IVerification>();

		/// <summary>
		/// 邮件接收人
		/// </summary>
		public List<string> EmailTo { get; set; }

		/// <summary>
		/// 邮件的标题
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// 爬虫任务的描述
		/// </summary>
		public string Description;

		/// <summary>
		/// 构造方法
		/// </summary>
		protected BaseVerification(SpiderOptions options)
		{
			Options = options;
		}

		/// <summary>
		/// 添加相等的判断
		/// </summary>
		/// <param name="name">规则名称</param>
		/// <param name="sql">SQL语句, 必须包含Result结果, 如: SELECT COUNT(*) AS Result FROM db.table1</param>
		/// <param name="value">期望等于的值</param>
		public void AddSqlEqual(string name, string sql, dynamic value)
		{
			Verifications.Add(new SqlEqual(name, sql, value));
		}

		/// <summary>
		/// 添加大于的判断
		/// </summary>
		/// <param name="name">规则名称</param>
		/// <param name="sql">SQL语句, 必须包含Result结果, 如: SELECT COUNT(*) AS Result FROM db.table1</param>
		/// <param name="value">期望大于的值</param>
		public void AddSqlLarge(string name, string sql, dynamic value)
		{
			Verifications.Add(new SqlLarge(name, sql, value));
		}

		/// <summary>
		/// 添加小于的判断
		/// </summary>
		/// <param name="name">规则名称</param>
		/// <param name="sql">SQL语句, 必须包含Result结果, 如: SELECT COUNT(*) AS Result FROM db.table1</param>
		/// <param name="value">期望小于的值</param>
		public void AddSqlLess(string name, string sql, dynamic value)
		{
			Verifications.Add(new SqlLess(name, sql, value));
		}

		/// <summary>
		/// 添加范围的判断
		/// </summary>
		/// <param name="name">规则名称</param>
		/// <param name="sql">SQL语句, 必须包含Result结果, 如: SELECT COUNT(*) AS Result FROM db.table1</param>
		/// <param name="minValue">期望的最小值</param>
		/// <param name="maxValue">期望的最大值</param>
		public void AddSqlRange(string name, string sql, dynamic minValue, dynamic maxValue)
		{
			Verifications.Add(new SqlRange(name, sql, minValue, maxValue));
		}

		/// <summary>
		/// 添加相等的判断, 用于如数据存在内存中
		/// </summary>
		/// <param name="name">规则名称</param>
		/// <param name="actual">真实值</param>
		/// <param name="expected">期望值</param>
		public void AddValueEqual(string name, dynamic actual, dynamic expected)
		{
			Verifications.Add(new ValueEqual(name, actual, expected));
		}

		/// <summary>
		/// 添加大于的判断, 用于如数据存在内存中
		/// </summary>
		/// <param name="name">规则名称</param>
		/// <param name="actual">真实值</param>
		/// <param name="value">期望大于的值</param>
		public void AddValueLarge(string name, dynamic actual, dynamic value)
		{
			Verifications.Add(new ValueLarge(name, actual, value));
		}

		/// <summary>
		/// 添加小于的判断, 用于如数据存在内存中
		/// </summary>
		/// <param name="name">规则名称</param>
		/// <param name="actual">真实值</param>
		/// <param name="value">期望小于的值</param>
		public void AddValueLess(string name, dynamic actual, dynamic value)
		{
			Verifications.Add(new ValueLess(name, actual, value));
		}

		/// <summary>
		/// 添加范围的判断, 用于如数据存在内存中
		/// </summary>
		/// <param name="name">规则名称</param>
		/// <param name="actual">真实值</param>
		/// <param name="minValue">期望的最小值</param>
		/// <param name="maxValue">期望的最大值</param>
		public void AddValueRange(string name, dynamic actual, dynamic minValue, dynamic maxValue)
		{
			Verifications.Add(new ValueRange(name, actual, minValue, maxValue));
		}

		/// <summary>
		/// 验证并发送报告
		/// </summary>
		/// <returns>验证的最终结果</returns>
		public abstract Task VerifyAsync(IDbConnection conn);

		abstract class BaseSqlVerification : IVerification
		{
			public string Name { get; protected set; }
			public string VerificationName { get; protected set; }
			public string Sql { get; protected set; }
			protected dynamic[] Values { get; set; }

			public async Task<VerificationMessage> VerifyAsync(params dynamic[] args)
			{
				if (!(args.ElementAtOrDefault(0) is IDbConnection))
				{
					throw new SpiderException("sql verification need a db connection from args");
				}

				bool verifyResult;
				Object result;
				string color;
				string verifyResultStr;

				try
				{
					var conn = (IDbConnection) args[0];
					result = await conn.QueryFirstOrDefaultAsync<dynamic>(Sql);
					verifyResult = Validate(result);
					color = verifyResult ? "forestgreen" : "orangered";
					verifyResultStr = verifyResult ? "PASS" : "FAILED";
				}
				catch (Exception e)
				{
					result = e.ToString();
					verifyResult = false;
					color = "orangered";
					verifyResultStr = "FAILED";
				}

				var report =
					"<tr>" +
					$"<td>{Name}</td>" +
					$"<td>{VerificationName}</td>" +
					$"<td>{Sql}</td>" +
					$"<td>{ExpectedValue}</td>" +
					$"<td>{result}</td>" +
					$"<td style=\"color:{color}\"><strong>{verifyResultStr}</strong></td>" +
					$"<td>{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}</td>" +
					"</tr>";

				return new VerificationMessage
				{
					Success = verifyResult,
					Message = report
				};
			}

			public abstract dynamic ExpectedValue { get; }

			public abstract bool Validate(dynamic result);
		}

		class SqlEqual : BaseSqlVerification
		{
			public SqlEqual(string name, string sql, dynamic value)
			{
				Sql = sql;
				Values = new[] {value};
				Name = name;
				VerificationName = "SQLEqual";
			}

			public override dynamic ExpectedValue => Values[0];

			public override bool Validate(dynamic result)
			{
				return result == ExpectedValue;
			}
		}

		class SqlLarge : BaseSqlVerification
		{
			public SqlLarge(string name, string sql, dynamic value)
			{
				Name = name;
				Sql = sql;
				VerificationName = "SQLLarge";
				Values = new[] {value};
			}

			public override dynamic ExpectedValue => Values[0];

			public override bool Validate(dynamic result)
			{
				return result > ExpectedValue;
			}
		}

		class SqlLess : BaseSqlVerification
		{
			public SqlLess(string name, string sql, dynamic value)
			{
				Name = name;
				Sql = sql;
				VerificationName = "SQLLess";
				Values = new[] {value};
			}

			public override dynamic ExpectedValue => Values[0];

			public override bool Validate(dynamic result)
			{
				return result < ExpectedValue;
			}
		}

		class SqlRange : BaseSqlVerification
		{
			public SqlRange(string name, string sql, dynamic minValue, dynamic maxValue)
			{
				Name = name;
				Sql = sql;
				VerificationName = "SQLRange";
				Values = new[] {minValue, maxValue};
			}

			public override dynamic ExpectedValue => $"{Values[0]}-{Values[1]}";

			public override bool Validate(dynamic result)
			{
				return result >= Values[0] && result <= Values[1];
			}
		}

		abstract class BaseValueVerification : IVerification
		{
			public string Name { get; protected set; }
			public string VerificationName { get; protected set; }

			public dynamic Acutal { get; protected set; }
			protected dynamic[] Expected { get; set; }

			public Task<VerificationMessage> VerifyAsync(params dynamic[] args)
			{
				bool verifyResult;
				string color;
				string verifyResultStr;

				try
				{
					verifyResult = Check(Acutal);
					color = verifyResult ? "forestgreen" : "orangered";
					verifyResultStr = verifyResult ? "PASS" : "FAILED";
				}
				catch (Exception e)
				{
					Acutal = e.ToString();
					verifyResult = false;
					color = "orangered";
					verifyResultStr = "FAILED";
				}

				var report =
					"<tr>" +
					$"<td>{Name}</td>" +
					$"<td>{VerificationName}</td>" +
					"<td>NONE</td>" +
					$"<td>{ExpectedValue}</td>" +
					$"<td>{Acutal}</td>" +
					$"<td style=\"color:{color}\"><strong>{verifyResultStr}</strong></td>" +
					$"<td>{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}</td>" +
					"</tr>";

				return Task.FromResult(new VerificationMessage
				{
					Success = verifyResult,
					Message = report
				});
			}

			public abstract dynamic ExpectedValue { get; }

			public abstract bool Check(dynamic result);
		}

		class ValueEqual : BaseValueVerification
		{
			public ValueEqual(string name, dynamic actual, dynamic expected)
			{
				Expected = new[] {expected};
				Name = name;
				Acutal = actual;
				VerificationName = "ValueEqual";
			}

			public override dynamic ExpectedValue => Expected[0];

			public override bool Check(dynamic result)
			{
				return result == ExpectedValue;
			}
		}

		class ValueLarge : BaseValueVerification
		{
			public ValueLarge(string name, dynamic actual, dynamic expected)
			{
				Name = name;
				Acutal = actual;
				VerificationName = "ValueLarge";
				Expected = new[] {expected};
			}

			public override dynamic ExpectedValue => Expected[0];

			public override bool Check(dynamic result)
			{
				return result > ExpectedValue;
			}
		}

		class ValueLess : BaseValueVerification
		{
			public ValueLess(string name, dynamic actual, dynamic expected)
			{
				Name = name;
				Acutal = actual;
				VerificationName = "ValueLess";
				Expected = new[] {expected};
			}

			public override dynamic ExpectedValue => Expected[0];

			public override bool Check(dynamic result)
			{
				return result < ExpectedValue;
			}
		}

		class ValueRange : BaseValueVerification
		{
			public ValueRange(string name, dynamic actual, dynamic minValue, dynamic maxValue)
			{
				Name = name;
				Acutal = actual;
				VerificationName = "ValueRange";
				Expected = new[] {minValue, maxValue};
			}

			public override dynamic ExpectedValue => $"{Expected[0]}-{Expected[1]}";

			public override bool Check(dynamic result)
			{
				return result >= Expected[0] && result <= Expected[1];
			}
		}
	}

	/// <summary>
	/// 验证类, 验证报告中可以在内容中插入一段样列数据
	/// </summary>
	public class Verification : BaseVerification
	{
		/// <summary>
		/// 验证报告中可以在内容中插入一段样列数据
		/// </summary>
		public string SampleSql { get; set; }

		/// <summary>
		/// 验证报告附件的数据
		/// </summary>
		private string ExportDataSql { get; set; }

		/// <summary>
		/// 验证报告数据的名件名
		/// </summary>
		private string ExportDataFile { get; set; }

		/// <summary>
		/// 构造方法, 验证报告中可以在内容中插入一段样列数据
		/// </summary>
		public Verification(SpiderOptions options) : base(options)
		{
		}

		/// <summary>
		/// 验证并发送报告
		/// </summary>
		/// <returns>验证的最终结果</returns>
		public override async Task VerifyAsync(IDbConnection conn)
		{
			if (string.IsNullOrWhiteSpace(Subject))
			{
				throw new SpiderException("Subject of verification should not be empty");
			}

			if (EmailTo == null || EmailTo.Count == 0)
			{
				throw new SpiderException("Should be more than 1 email");
			}

			if (Verifications != null && Verifications.Count > 0 &&
			    !string.IsNullOrWhiteSpace(Options.EmailHost))
			{
				var emailBody = new StringBuilder();
				emailBody.Append(
					"<html><head>" +
					"<meta charset=\"utf-8\">" +
					"<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
					"<meta name=\"viewport\" content=\"width=device-width initial-scale=1.0\">" +
					$"<title>{Subject}: {DateTimeOffset.Now}</title>" +
					"<style>" +
					"table {border-collapse: collapse;border-spacing: 0;border-left: 1px solid #888;border-top: 1px solid #888;background: #efefef;}th, td {border-right: 1px solid #888;border-bottom: 1px solid #888;padding: 5px 15px;}th {font-weight: bold;background: #ccc;}" +
					"</style>" +
					"</head>" +
					"<body style=\"background-color:#FAF7EC\">" +
					$"<h2>{Subject}: {DateTimeOffset.Now}</h2>" +
					$"&nbsp;&nbsp;&nbsp;<strong>Description: </strong>{Description}" +
					"<br/><br/>" +
					"<table>" +
					"<thead>" +
					"<tr>" +
					"<th>Item</th>" +
					"<th>Rule</th>" +
					"<th>SQL</th>" +
					"<th>Expected</th>" +
					"<th>Actual</th>" +
					"<th>Result</th>" +
					"<th>Time</th> " +
					"</tr>" +
					"</thead>" +
					"<tbody>"
				);
				var success = true;
				foreach (var verifier in Verifications)
				{
					var result = await verifier.VerifyAsync(conn);
					emailBody.AppendLine(result.Message);
					if (success && !result.Success)
					{
						success = false;
					}
				}


				emailBody.Append("</tbody></table><br/>");
				if (!string.IsNullOrWhiteSpace(SampleSql))
				{
					emailBody.Append("<strong>数据样本</strong><br/><br/>");
					emailBody.Append(conn.ToHtml($"{SampleSql} LIMIT 100;"));
				}

				emailBody.Append("<br/><br/></body></html>");

				var message = new MimeMessage();
				var displayName = string.IsNullOrWhiteSpace(Options.EmailDisplayName)
					? "DotnetSpider Alert"
					: Options.EmailDisplayName;
				message.From.Add(new MailboxAddress(displayName, Options.EmailAccount));
				foreach (var emailTo in EmailTo)
				{
					message.To.Add(new MailboxAddress(emailTo, emailTo));
				}

				message.Subject = $"{Subject}: {(success ? "Success" : "Failed")}";

				var html = new TextPart("html")
				{
#if NETFRAMEWORK
					Text = System.Net.WebUtility.HtmlDecode(emailBody.ToString())
#else
					Text = HttpUtility.HtmlDecode(emailBody.ToString())
#endif
				};
				var multipart = new Multipart("mixed") {html};

				if (success && !string.IsNullOrWhiteSpace(ExportDataSql) &&
				    !string.IsNullOrWhiteSpace(ExportDataFile))
				{
					var path = conn.Export(ExportDataSql, $"{ExportDataFile}_{DateTimeOffset.Now:yyyyMMddhhmmss}",
						true);
					var attachment = new MimePart("excel", "xlsx")
					{
#if !NET40
						Content = new MimeContent(File.OpenRead(path)),
#else
							ContentObject = new ContentObject(File.OpenRead(path)),
#endif
						ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
						ContentTransferEncoding = ContentEncoding.Base64,
						FileName = Path.GetFileName(path)
					};
					multipart.Add(attachment);
				}

				message.Body = multipart;

				using (var client = new SmtpClient())
				{
					await client.ConnectAsync(Options.EmailHost, Options.EmailPort);

					// Note: only needed if the SMTP server requires authentication
					await client.AuthenticateAsync(Options.EmailAccount, Options.EmailPassword);

					await client.SendAsync(message);
					await client.DisconnectAsync(true);
				}
			}
		}
	}
}
