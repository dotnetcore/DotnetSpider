using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OfficeOpenXml;
using System.IO;
using MimeKit;

namespace DotnetSpider.Extension.Infrastructure
{
	public static class ExcelExtensions
	{
		public static string Export(this IDbConnection conn, string sql, string fileName, bool rewrite = false)
		{
			var command = conn.CreateCommand();
			command.CommandText = sql;
			command.CommandType = CommandType.Text;

			if (conn.State == ConnectionState.Closed)
			{
				conn.Open();
			}

			IDataReader reader = null;

			try
			{
				reader = command.ExecuteReader();

				int row = 1;
				using (var p = new ExcelPackage())
				{
					var sheet = p.Workbook.Worksheets.Add("Sheet1");
					while (reader.Read())
					{
						if (row == 1)
						{
							for (int i = 1; i < reader.FieldCount + 1; ++i)
							{
								sheet.Cells[1, i].Value = reader.GetName(i - 1);
							}
						}

						var realRowIndx = row + 1;
						for (int j = 1; j < reader.FieldCount + 1; ++j)
						{
							sheet.Cells[realRowIndx, j].Value = reader.GetValue(j - 1);
						}

						row++;
					}
					var folder = Path.Combine(Core.Environment.GlobalDirectory, "excels");
					if (!Directory.Exists(folder))
					{
						Directory.CreateDirectory(folder);
					}
					var path = Path.Combine(Core.Environment.GlobalDirectory, "excels", $"{fileName}.xlsx");
					if (File.Exists(path) && rewrite)
					{
						File.Delete(path);
					}
					p.SaveAs(new FileInfo(path));
					return path;
				}
			}
			finally
			{
				reader?.Close();
			}
		}

		public static void EmailTo(this IDbConnection conn, string sql, string fileName, string subject, string emailTo)
		{
			EmailTo(conn, sql, fileName, subject, emailTo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList(), Core.Environment.EmailHost, int.Parse(Core.Environment.EmailPort), Core.Environment.EmailAccount, Core.Environment.EmailPassword, Core.Environment.EmailDisplayName);
		}

		public static void EmailTo(this IDbConnection conn, string sql, string fileName, string subject, List<string> emailTo)
		{
			EmailTo(conn, sql, fileName, subject, emailTo, Core.Environment.EmailHost, int.Parse(Core.Environment.EmailPort), Core.Environment.EmailAccount, Core.Environment.EmailPassword, Core.Environment.EmailDisplayName);
		}

		public static void EmailTo(this IDbConnection conn, string sql, string fileName, string subject, string emailTo, string emailHost, int port, string account, string password, string displayName = "DotnetSpider Alert")
		{
			EmailTo(conn, sql, fileName, subject, emailTo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList(), emailHost, port, account, password, displayName);
		}

		public static void EmailTo(this IDbConnection conn, string sql, string fileName, string subject, List<string> emailTo, string emailHost, int port, string account, string password, string displayName = "DotnetSpider Alert")
		{
			var path = Export(conn, sql, $"{fileName}_{DateTime.Now:yyyyMMddhhmmss}", true);
			var message = new MimeMessage();

			message.From.Add(new MailboxAddress(displayName, account));
			foreach (var email in emailTo)
			{
				message.To.Add(new MailboxAddress(email, email));
			}

			message.Subject = subject;

			var attachment = new MimePart("excel", "xlsx")
			{
				ContentObject = new ContentObject(File.OpenRead(path)),
				ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
				ContentTransferEncoding = ContentEncoding.Base64,
				FileName = Path.GetFileName(path)
			};

			var text = new TextPart { Text = subject };


			var multipart = new Multipart("mixed") { text, attachment };

			message.Body = multipart;

			using (var client = new MailKit.Net.Smtp.SmtpClient())
			{
				client.Connect(emailHost, port);

				client.Authenticate(account, password);

				client.Send(message);
				client.Disconnect(true);
			}
		}
	}
}
