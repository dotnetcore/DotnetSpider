using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;
using DotnetSpider.Core.Infrastructure;
using MySql.Data.MySqlClient;
using MimeKit;
using System.Linq;
using System.Reflection;
using System.Threading;
using NLog;
using System.Web;

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
		string VerifierName { get; }
		VerificationInfo Verify(IDbConnection conn);
	}

	public abstract class Verification
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

		protected Verification()
		{
			EmailHost = Config.GetValue("emailHost")?.Trim();
			var portStr = Config.GetValue("emailPort");
			if (!string.IsNullOrEmpty(portStr))
			{
				int port;
				if (int.TryParse(portStr, out port))
				{
					EmailPort = port;
				}
				else
				{
					Logger.MyLog($"EmailPort is not a number: {portStr}.", LogLevel.Error);
				}
			}
			EmailAccount = Config.GetValue("emailAccount");
			EmailPassword = Config.GetValue("emailPassword");
			EmailDisplayName = Config.GetValue("emailDisplayName");
		}

		protected Verification(string emailTo, string subject, string host, int port, string account, string password)
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

		public static void ProcessVerifidation(string identity, Action verify)
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
					needVerify = lockerValue != "verify finished";
				}
				if (needVerify)
				{
					Logger.MyLog(identity, "Start data verification...", LogLevel.Info);
					verify();
					Logger.MyLog(identity, "Data verification complete.", LogLevel.Info);
				}
				else
				{
					Logger.MyLog(identity, "Data verification is done already.", LogLevel.Info);
				}

				if (needVerify)
				{
					RedisConnection.Default?.Database.HashSet(ValidateStatusKey, identity, "verify finished");
				}
			}
			catch (Exception e)
			{
				Logger.MyLog(identity, e.Message, LogLevel.Error, e);
				//throw;
			}
			finally
			{
				RedisConnection.Default?.Database.LockRelease(key, 0);
			}
		}

		public void AddEqual(string name, string sql, dynamic value)
		{
			Verifiers.Add(new Equal(name, sql, value));
		}

		public void AddLarge(string name, string sql, dynamic value)
		{
			Verifiers.Add(new Large(name, sql, value));
		}

		public void AddLess(string name, string sql, dynamic value)
		{
			Verifiers.Add(new Less(name, sql, value));
		}

		public void AddRange(string name, string sql, dynamic minValue, dynamic maxValue)
		{
			Verifiers.Add(new Range(name, sql, minValue, maxValue));
		}

		public abstract VerificationResult Report(string sql = null);

		abstract class BaseVerifier : IVerification
		{
			public string Name { get; protected set; }
			public string VerifierName { get; protected set; }
			protected string Sql { get; set; }
			protected dynamic[] Values { get; set; }

			public VerificationInfo Verify(IDbConnection conn)
			{
				bool verifyResult;
				Object result;
				string color;
				string verifyResultStr;

				try
				{
					var query = conn.Query<QueryResult>(Sql).FirstOrDefault();
					result = query?.Result;
					verifyResult = Verify(result);
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
				$"<td>{VerifierName}</td>" +
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

			public abstract bool Verify(dynamic result);
		}

		class Equal : BaseVerifier
		{
			public Equal(string name, string sql, dynamic value)
			{
				Sql = sql;
				Values = new[] { value };
				Name = name;
				VerifierName = "Equal";
			}

			public override dynamic ExpectedValue => Values[0];

			public override bool Verify(dynamic result)
			{
				return result == ExpectedValue;
			}
		}

		class Large : BaseVerifier
		{
			public Large(string name, string sql, dynamic value)
			{
				Name = name;
				Sql = sql;
				VerifierName = "Large";
				Values = new[] { value };
			}

			public override dynamic ExpectedValue => Values[0];

			public override bool Verify(dynamic result)
			{
				return result > ExpectedValue;
			}
		}

		class Less : BaseVerifier
		{
			public Less(string name, string sql, dynamic value)
			{
				Name = name;
				Sql = sql;
				VerifierName = "Less";
				Values = new[] { value };
			}

			public override dynamic ExpectedValue => Values[0];

			public override bool Verify(dynamic result)
			{
				return result < ExpectedValue;
			}
		}

		class Range : BaseVerifier
		{
			public Range(string name, string sql, dynamic minValue, dynamic maxValue)
			{
				Name = name;
				Sql = sql;
				VerifierName = "Range";
				Values = new[] { minValue, maxValue };
			}

			public override dynamic ExpectedValue => $"{Values[0]}-{Values[1]}";

			public override bool Verify(dynamic result)
			{
				return result >= Values[0] && result <= Values[1];
			}
		}
	}

	public class VerificationResult
	{
		public bool PassVeridation { get; set; }
	}

	public class Verification<TE> : Verification
	{
		public Properties Properties { get; }

		public Verification()
		{
			Properties = typeof(TE).GetTypeInfo().GetCustomAttribute<Properties>();
			EmailTo = Properties.Email?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
			Subject = Properties.Subject;
		}

		public Verification(string emailTo, string subject, string host, int port, string account, string password) : base(emailTo, subject, host, port, account, password)
		{
			Properties = typeof(TE).GetTypeInfo().GetCustomAttribute<Properties>();
		}

		public override VerificationResult Report(string sql = null)
		{
			VerificationResult veridationResult = new VerificationResult();
			if (!string.IsNullOrEmpty(sql) && sql.ToLower().Contains("limit"))
			{
				Logger.MyLog("SQL contains 'LIMIT'.", LogLevel.Error);
				return veridationResult;
			}
			if (Verifiers != null && Verifiers.Count > 0 && EmailTo != null && EmailTo.Count > 0 && !string.IsNullOrEmpty(EmailHost))
			{
				using (var conn = new MySqlConnection(Config.ConnectString))
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
					if (!string.IsNullOrEmpty(sql))
					{
						emailBody.Append("<strong>数据样本</strong><br/><br/>");
						emailBody.Append(conn.ToHTML($"{sql} LIMIT 100;"));
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

					message.Body = html;

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
}
