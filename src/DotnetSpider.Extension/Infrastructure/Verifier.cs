using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;
using DotnetSpider.Core.Infrastructure;
using MySql.Data.MySqlClient;
using MimeKit;
using System.Linq;

namespace DotnetSpider.Extension.Infrastructure
{
	public class Verifier
	{
		private List<IVerifier> verifiers = new List<IVerifier>();

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

		public void AddEqual<T>(string name, string sql, T value)
		{
			verifiers.Add(new Equal<T>(name, sql, value));
		}

		public void AddLarge<T>(string name, string sql, T value)
		{
			verifiers.Add(new Large<T>(name, sql, value));
		}

		public void AddLess<T>(string name, string sql, T value)
		{
			verifiers.Add(new Less<T>(name, sql, value));
		}

		public void AddRange<T>(string name, string minSql, string maxSql, T minValue, T maxValue)
		{
			verifiers.Add(new Range<T>(name, minSql, maxSql, minValue, maxValue));
		}

		public void Build()
		{
			if (verifiers != null && verifiers.Count > 0 && EmailTo != null && EmailTo.Count > 0)
			{
				using (var conn = new MySqlConnection(Config.ConnectString))
				{
					var emailBody = new StringBuilder();
					emailBody.Append($"HEADER{System.Environment.NewLine}");
					foreach (var verifier in verifiers)
					{
						emailBody.AppendLine(verifier.Verify(conn));
					}
					emailBody.Append($"FOOTER{System.Environment.NewLine}");

					var message = new MimeMessage();
					message.From.Add(new MailboxAddress("DotnetSpider Verifier", "verifier@dotnetspider.com"));
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
						client.Connect(EmailHost, EmailPort, false);

						// Note: only needed if the SMTP server requires authentication
						client.Authenticate(EmailAccount, EmailPassword);

						client.Send(message);
						client.Disconnect(true);
					}
				}
			}
		}

		interface IVerifier
		{
			string Name { get; }
			string Verify(IDbConnection conn);
		}

		class Equal<T> : IVerifier
		{
			public string Sql { get; }
			public T Value { get; }
			public string Name { get; }

			public Equal(string name, string sql, T value)
			{
				Sql = sql;
				Value = value;
				Name = name;
			}

			public string Verify(IDbConnection conn)
			{
				return "";
			}
		}

		class Large<T> : IVerifier
		{
			public string Name { get; }
			public string Sql { get; }
			public T Value { get; }

			public Large(string name, string sql, T value)
			{
				Name = name;
				Sql = sql;
				Value = value;
			}

			public string Verify(IDbConnection conn)
			{
				return "";
			}
		}

		class Less<T> : IVerifier
		{
			public string Name { get; }
			public string Sql { get; }
			public T Value { get; }

			public Less(string name, string sql, T value)
			{
				Name = name;
				Sql = sql;
				Value = value;
			}

			public string Verify(IDbConnection conn)
			{
				return "";
			}
		}

		class Range<T> : IVerifier
		{
			public string Name { get; }
			public string MinSql { get; }
			public string MaxSql { get; }
			public T MinValue { get; }
			public T MaxValue { get; }

			public Range(string name, string minSql, string maxSql, T minValue, T maxValue)
			{
				Name = name;
				MinSql = minSql;
				MinValue = minValue;
				MaxSql = maxSql;
				MaxValue = maxValue;
			}

			public string Verify(IDbConnection conn)
			{
				return "";
			}
		}

		class QueryResult
		{
			public dynamic Result { get; set; }
		}
	}
}
