using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DotnetSpider.Core.Infrastructure;
using MimeKit;
using System.Linq;
using System.Reflection;
using System.Threading;
using NLog;
using System.Web;
using System.IO;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension.Infrastructure
{
	internal class QueryResult
	{
		public dynamic Result { get; set; }
	}

	public class VerificationInfo
	{
		public bool Pass { get; set; }
		public string Report { get; set; }
	}

	public interface IVerification
	{
		string Name { get; }
		string VerificationName { get; }
		VerificationInfo Verify(IDbConnection conn);
	}

	public abstract class BaseVerification
	{
		protected static readonly ILogger Logger = LogCenter.GetLogger();
		protected const string ValidateStatusKey = "dotnetspider:validate-stats";
		protected List<IVerification> Verifiers = new List<IVerification>();

		public List<string> EmailTo { get; set; }
		public string EmailHost { get; set; }
		public string Subject { get; set; }
		public string Description { get; set; }
		public int EmailPort { get; set; } = 25;
		public string EmailAccount { get; set; }
		public string EmailPassword { get; set; }

		public string EmailDisplayName { get; set; }

		protected BaseVerification()
		{
			EmailHost = Core.Env.EmailHost;
			var portStr = Core.Env.EmailPort;
			if (!string.IsNullOrEmpty(portStr))
			{
				if (int.TryParse(portStr, out var port))
				{
					EmailPort = port;
				}
				else
				{
					Logger.AllLog($"EmailPort is not a number: {portStr}.", LogLevel.Error);
				}
			}
			EmailAccount = Core.Env.EmailAccount;
			EmailPassword = Core.Env.EmailPassword;
			EmailDisplayName = Core.Env.EmailDisplayName;
		}

		protected BaseVerification(string emailTo, string subject, string host, int port, string account, string password)
		{
			EmailTo = emailTo.Split(';').Select(e => e.Trim()).ToList();
			EmailHost = host;
			EmailPort = port;
			EmailAccount = account;
			EmailPassword = password;
			Subject = subject;
		}

		public static void RemoveVerifidationLock(string identity)
		{
			RedisConnection.Default?.Database.HashDelete(ValidateStatusKey, identity);
		}

		public static void ProcessVerifidation(string identity, Action dataVerificationAndReport)
		{
			string key = $"dotnetspider:validateLocker:{identity}";

			try
			{
				bool needVerify = true;
				if (RedisConnection.Default != null)
				{
					while (!RedisConnection.Default.Database.LockTake(key, "0", TimeSpan.FromMinutes(10)))
					{
						Thread.Sleep(1000);
					}

					var lockerValue = RedisConnection.Default.Database.HashGet(ValidateStatusKey, identity);
					needVerify = lockerValue != "verify completed.";
				}
				if (needVerify)
				{
					Logger.AllLog(identity, "Start data verification...", LogLevel.Info);
					dataVerificationAndReport();
					Logger.AllLog(identity, "Data verification complete.", LogLevel.Info);
				}
				else
				{
					Logger.AllLog(identity, "Data verification is done already.", LogLevel.Info);
				}

				if (needVerify)
				{
					RedisConnection.Default?.Database.HashSet(ValidateStatusKey, identity, "verify completed.");
				}
			}
			catch (Exception e)
			{
				Logger.AllLog(identity, e.Message, LogLevel.Error, e);
			}
			finally
			{
				RedisConnection.Default?.Database.LockRelease(key, 0);
			}
		}

		public void AddSqlEqual(string name, string sql, dynamic value)
		{
			Verifiers.Add(new SqlEqual(name, sql, value));
		}

		public void AddSqlLarge(string name, string sql, dynamic value)
		{
			Verifiers.Add(new SqlLarge(name, sql, value));
		}

		public void AddSqlLess(string name, string sql, dynamic value)
		{
			Verifiers.Add(new SqlLess(name, sql, value));
		}

		public void AddSqlRange(string name, string sql, dynamic minValue, dynamic maxValue)
		{
			Verifiers.Add(new SqlRange(name, sql, minValue, maxValue));
		}

		public void AddValueEqual(string name, dynamic actual, dynamic expected)
		{
			Verifiers.Add(new ValueEqual(name, actual, expected));
		}

		public void AddValueLarge(string name, dynamic actual, dynamic value)
		{
			Verifiers.Add(new ValueLarge(name, actual, value));
		}

		public void AddValueLess(string name, dynamic actual, dynamic value)
		{
			Verifiers.Add(new ValueLess(name, actual, value));
		}

		public void AddValueRange(string name, dynamic actual, dynamic minValue, dynamic maxValue)
		{
			Verifiers.Add(new ValueRange(name, actual, minValue, maxValue));
		}

		public abstract VerificationResult Report();

		abstract class BaseSqlVerification : IVerification
		{
			public string Name { get; protected set; }
			public string VerificationName { get; protected set; }
			public string Sql { get; protected set; }
			protected dynamic[] Values { get; set; }

			public VerificationInfo Verify(IDbConnection conn)
			{
				bool verifyResult;
				Object result;
				string color;
				string verifyResultStr;

				try
				{
					var query = conn.MyQuery<QueryResult>(Sql).FirstOrDefault();
					result = query?.Result;
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
				$"<td>{DateTime.Now:yyyy-MM-dd hh:mm:ss}</td>" +
				"</tr>";

				return new VerificationInfo
				{
					Pass = verifyResult,
					Report = report
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
				Values = new[] { value };
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
				Values = new[] { value };
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
				Values = new[] { value };
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
				Values = new[] { minValue, maxValue };
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

			public VerificationInfo Verify(IDbConnection conn)
			{
				bool verifyResult;
				string color;
				string verifyResultStr;

				try
				{
					verifyResult = Validate(Acutal);
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
				$"<td>NONE</td>" +
				$"<td>{ExpectedValue}</td>" +
				$"<td>{Acutal}</td>" +
				$"<td style=\"color:{color}\"><strong>{verifyResultStr}</strong></td>" +
				$"<td>{DateTime.Now:yyyy-MM-dd hh:mm:ss}</td>" +
				"</tr>";

				return new VerificationInfo
				{
					Pass = verifyResult,
					Report = report
				};
			}

			public abstract dynamic ExpectedValue { get; }

			public abstract bool Validate(dynamic result);
		}

		class ValueEqual : BaseValueVerification
		{
			public ValueEqual(string name, dynamic actual, dynamic expected)
			{
				Expected = new[] { expected };
				Name = name;
				Acutal = actual;
				VerificationName = "ValueEqual";
			}

			public override dynamic ExpectedValue => Expected[0];

			public override bool Validate(dynamic result)
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
				Expected = new[] { expected };
			}

			public override dynamic ExpectedValue => Expected[0];

			public override bool Validate(dynamic result)
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
				Expected = new[] { expected };
			}

			public override dynamic ExpectedValue => Expected[0];

			public override bool Validate(dynamic result)
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
				Expected = new[] { minValue, maxValue };
			}

			public override dynamic ExpectedValue => $"{Expected[0]}-{Expected[1]}";

			public override bool Validate(dynamic result)
			{
				return result >= Expected[0] && result <= Expected[1];
			}
		}
	}

	public class VerificationResult
	{
		public bool PassVeridation { get; set; }
	}

	public class Verification : BaseVerification
	{
		public Description Properties { get; }

		public string ReportSampleSql { get; set; }

		public string ExportDataSql { get; set; }

		public string ExportDataFileName { get; set; }

		public Verification(Type type, string reportSampleSql = null)
		{
			Properties = type.GetTypeInfo().GetCustomAttribute<Description>();
			EmailTo = Properties.Email?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
			Subject = Properties.Subject;
			ReportSampleSql = reportSampleSql;
		}

		public Verification(Type type, string emailTo, string subject, string host, int port, string account, string password) : base(emailTo, subject, host, port, account, password)
		{
			Properties = type.GetTypeInfo().GetCustomAttribute<Description>();
		}

		public override VerificationResult Report()
		{
			VerificationResult veridationResult = new VerificationResult();
			if (Core.Env.SystemConnectionStringSettings == null)
			{
				return veridationResult;
			}
			if (!string.IsNullOrEmpty(ReportSampleSql) && ReportSampleSql.ToLower().Contains("limit"))
			{
				Logger.AllLog("SQL contains 'LIMIT'.", LogLevel.Error);
				return veridationResult;
			}
			if (Verifiers != null && Verifiers.Count > 0 && EmailTo != null && EmailTo.Count > 0 && !string.IsNullOrEmpty(EmailHost))
			{
				using (var conn = Core.Env.DataConnectionStringSettings.CreateDbConnection())
				{
					var emailBody = new StringBuilder();
					var hasProperties = Properties != null;
					emailBody.Append(
"<html><head>" +
"<meta charset=\"utf-8\">" +
"<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
"<meta name=\"viewport\" content=\"width=device-width initial-scale=1.0\">" +
$"<title>{Subject}: {DateTime.Now}</title>" +
"<style>" +
"table {border-collapse: collapse;border-spacing: 0;border-left: 1px solid #888;border-top: 1px solid #888;background: #efefef;}th, td {border-right: 1px solid #888;border-bottom: 1px solid #888;padding: 5px 15px;}th {font-weight: bold;background: #ccc;}" +
"</style>" +
"</head>" +
"<body style=\"background-color:#FAF7EC\">" +
$"<h2>{Subject}: {DateTime.Now}</h2>" +
(hasProperties ? $"<strong>Analyst: </strong>{Properties.Owner}" : "") +
(hasProperties ? $"&nbsp;&nbsp;&nbsp;<strong>Developer: </strong>{Properties.Developer}" : "") +
(hasProperties ? $"&nbsp;&nbsp;&nbsp;<strong>Date: </strong>{Properties.Date}" : "") +
(hasProperties ? $"&nbsp;&nbsp;&nbsp;<strong>Description: </strong>{Description}" : "") +
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
					foreach (var verifier in Verifiers)
					{
						var result = verifier.Verify(conn);
						emailBody.AppendLine(result.Report);
						if (success && !result.Pass)
						{
							success = false;
						}
					}
					veridationResult.PassVeridation = success;
					emailBody.Append("</tbody></table><br/>");
					if (!string.IsNullOrEmpty(ReportSampleSql))
					{
						emailBody.Append("<strong>数据样本</strong><br/><br/>");
						emailBody.Append(conn.ToHtml($"{ReportSampleSql} LIMIT 100;"));
					}
					emailBody.Append("<br/><br/></body></html>");

					var message = new MimeMessage();
					var displayName = string.IsNullOrEmpty(EmailDisplayName) ? "DotnetSpider Alert" : EmailDisplayName;
					message.From.Add(new MailboxAddress(displayName, EmailAccount));
					foreach (var emailTo in EmailTo)
					{
						message.To.Add(new MailboxAddress(emailTo, emailTo));
					}

					message.Subject = $"{Subject}: {(success ? "Success" : "Failed")}";

					var html = new TextPart("html")
					{
						Text = HttpUtility.HtmlDecode(emailBody.ToString())
					};
					var multipart = new Multipart("mixed") { html };

					if (veridationResult.PassVeridation && !string.IsNullOrEmpty(ExportDataSql) && !string.IsNullOrEmpty(ExportDataFileName))
					{
						var path = conn.Export(ExportDataSql, $"{ExportDataFileName}_{DateTime.Now:yyyyMMddhhmmss}", true);
						var attachment = new MimePart("excel", "xlsx")
						{
							ContentObject = new ContentObject(File.OpenRead(path)),
							ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
							ContentTransferEncoding = ContentEncoding.Base64,
							FileName = Path.GetFileName(path)
						};
						multipart.Add(attachment);
					}

					message.Body = multipart;

					using (var client = new MailKit.Net.Smtp.SmtpClient())
					{
						client.Connect(EmailHost, EmailPort);

						// Note: only needed if the SMTP server requires authentication
						client.Authenticate(EmailAccount, EmailPassword);

						client.Send(message);
						client.Disconnect(true);
					}
				}
			}


			return veridationResult;
		}
	}

	public class Verification<TE> : Verification
	{
		public Verification(string reportSampleSql = null) : base(typeof(TE), reportSampleSql)
		{
		}

		public Verification(string emailTo, string subject, string host, int port, string account, string password) : base(typeof(TE), emailTo, subject, host, port, account, password)
		{
		}

	}
}
