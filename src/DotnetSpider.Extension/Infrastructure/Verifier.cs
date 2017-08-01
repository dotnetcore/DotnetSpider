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
using MailKit.Security;

namespace DotnetSpider.Extension.Infrastructure
{
	public class QueryResult
	{
		public dynamic Result { get; set; }
	}

	public interface IVerifier
	{
		string Name { get; }
		string VerifierName { get; }
		string Verify(IDbConnection conn);
	}

	public abstract class Verifier
	{
		protected readonly static ILogger Logger = LogCenter.GetLogger();
		protected const string ValidateStatusKey = "dotnetspider:validate-stats";
		protected List<IVerifier> verifiers = new List<IVerifier>();

		public List<string> EmailTo { get; }
		public string EmailHost { get; }
		public string Subject { get; }
		public int EmailPort { get; }
		public string EmailAccount { get; }
		public string EmailPassword { get; }

		public Verifier(string emailTo, string subject)
		{
			EmailTo = emailTo.Split(';').Select(e => e.Trim()).ToList();
			EmailHost = Config.GetValue("emailHost");
			EmailPort = int.Parse(Config.GetValue("emailPort"));
			EmailAccount = Config.GetValue("emailAccount");
			EmailPassword = Config.GetValue("emailPassword");
			Subject = subject;
		}

		public Verifier(string emailTo, string subject, string host, int port, string account, string password)
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
					Logger.MyLog(identity, "开始执行数据验证...", LogLevel.Info);
					verify();
					Logger.MyLog(identity, "数据验证已完成.", LogLevel.Info);
				}
				else
				{
					Logger.MyLog(identity, "数据验证已完成, 请勿重复操作.", LogLevel.Info);
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
			verifiers.Add(new Equal(name, sql, value));
		}

		public void AddLarge(string name, string sql, dynamic value)
		{
			verifiers.Add(new Large(name, sql, value));
		}

		public void AddLess(string name, string sql, dynamic value)
		{
			verifiers.Add(new Less(name, sql, value));
		}

		public void AddRange(string name, string sql, dynamic minValue, dynamic maxValue)
		{
			verifiers.Add(new Range(name, sql, minValue, maxValue));
		}

		public abstract void Report();

		abstract class BaseVerifier : IVerifier
		{
			public string Name { get; protected set; }
			public string VerifierName { get; protected set; }
			public string Sql { get; protected set; }
			public dynamic[] Values { get; protected set; }

			public string Verify(IDbConnection conn)
			{
				QueryResult query;
				bool verifyResult;
				Object result;
				string color;
				string verifyResultStr;
				try
				{
					query = conn.Query<QueryResult>(Sql).FirstOrDefault();
					result = query != null ? query.Result : null;
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


				return
				"<tr>" +
				$"<td>{Name}</td>" +
				$"<td>{VerifierName}</td>" +
				$"<td>{Sql}</td>" +
				$"<td>{ExpectedValue}</td>" +
				$"<td>{result}</td>" +
				$"<td style=\"color:{color}\"><strong>{verifyResultStr}</strong></td>" +
				$"<td>{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}</td>" +
				"</tr>";
			}

			public abstract dynamic ExpectedValue { get; }

			public abstract bool Verify(dynamic result);
		}

		class Equal : BaseVerifier
		{
			public Equal(string name, string sql, dynamic value)
			{
				Sql = sql;
				Values = new dynamic[] { value };
				Name = name;
				VerifierName = "相等";
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
				VerifierName = "大于";
				Values = new dynamic[] { value };
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
				VerifierName = "小于";
				Values = new dynamic[] { value };
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
				VerifierName = "范围";
				Values = new dynamic[] { minValue, maxValue };
			}

			public override dynamic ExpectedValue => $"{Values[0]}-{Values[1]}";

			public override bool Verify(dynamic result)
			{
				return result >= Values[0] && result <= Values[1];
			}
		}
	}

	public class Verifier<E> : Verifier
	{
		public Properties Properties { get; }

		public Verifier(string emailTo, string subject) : base(emailTo, subject)
		{
			Properties = typeof(E).GetTypeInfo().GetCustomAttribute<Properties>();
		}

		public Verifier(string emailTo, string subject, string host, int port, string account, string password) : base(emailTo, subject, host, port, account, password)
		{
			Properties = typeof(E).GetTypeInfo().GetCustomAttribute<Properties>();
		}

		public override void Report()
		{
			if (verifiers != null && verifiers.Count > 0 && EmailTo != null && EmailTo.Count > 0 && !string.IsNullOrEmpty(EmailHost))
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
$"<title>{Subject}: {DateTime.Now.ToString()}</title>" +
"<style>" +
"table {border-collapse: collapse;border-spacing: 0;border-left: 1px solid #888;border-top: 1px solid #888;background: #efefef;}th, td {border-right: 1px solid #888;border-bottom: 1px solid #888;padding: 5px 15px;}th {font-weight: bold;background: #ccc;}" +
"</style>" +
"</head>" +
"<body style=\"background-color:#FAF7EC\">" +
$"<h2>{Subject}: {DateTime.Now.ToString()}</h2>" +
(hasProperties ? $"<p><strong>研究员: {Properties.Designer}</strong></p>" : "") +
(hasProperties ? $"<p><strong>爬虫负责人: {Properties.Developer}</strong></p>" : "") +
(hasProperties ? $"<p><strong>开发时间: {Properties.Date}</strong></p>" : "") +
(hasProperties ? $"<p><strong>任务描述: {Properties.Detail}</strong></p>" : "") +
"<br/>" +
"<table>" +
"<thead>" +
"<tr>" +
"<th>检查项</th>" +
"<th>规则</th>" +
"<th>SQL</th>" +
"<th>期望值</th>" +
"<th>真实值</th>" +
"<th>结果</th>" +
"<th>检测时间</th> " +
"</tr>" +
"</thead>" +
"<tbody>"
);
					foreach (var verifier in verifiers)
					{
						emailBody.AppendLine(verifier.Verify(conn));
					}
					emailBody.Append("</tbody></table><br/></body></html>");

					var message = new MimeMessage();
					message.From.Add(new MailboxAddress("DotnetSpider Verifier", EmailAccount));
					foreach (var emailTo in EmailTo)
					{
						message.To.Add(new MailboxAddress(emailTo, emailTo));
					}

					message.Subject = Subject;

					var html = new TextPart("html")
					{
						Text = emailBody.ToString()
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
		}


	}
}
