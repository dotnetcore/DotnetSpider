using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using MimeKit;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core;
using OfficeOpenXml;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// Excel扩展
	/// </summary>
	public static class ExcelExtensions
	{
		/// <summary>
		/// 导出数据库数据到EXCEL
		/// </summary>
		/// <param name="conn">数据库连接</param>
		/// <param name="sql">SQL语句</param>
		/// <param name="fileName">文件名</param>
		/// <param name="rewrite">是否覆盖旧文件</param>
		/// <returns></returns>
		public static string Export(this IDbConnection conn, string sql, string fileName, bool rewrite = false)
		{
			IDataReader reader = null;

			try
			{
				reader = conn.MyExecuteReader(sql);

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
					var folder = Path.Combine(Env.GlobalDirectory, "excels");
					if (!Directory.Exists(folder))
					{
						Directory.CreateDirectory(folder);
					}
					var path = Path.Combine(Env.GlobalDirectory, "excels", $"{fileName}.xlsx");
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

		/// <summary>
		/// 把数据库数据导出到EXCEL并发送邮件
		/// </summary>
		/// <param name="conn">数据库连接</param>
		/// <param name="sql">SQL语句</param>
		/// <param name="fileName">文件名</param>
		/// <param name="subject">邮件的标题</param>
		/// <param name="emailTo">邮件接收人</param>
		public static void EmailTo(this IDbConnection conn, string sql, string fileName, string subject, string emailTo)
		{
			EmailTo(conn, sql, fileName, subject, emailTo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList(), Env.EmailHost, int.Parse(Env.EmailPort), Env.EmailAccount, Env.EmailPassword, Env.EmailDisplayName);
		}

		/// <summary>
		/// 把数据库数据导出到EXCEL并发送邮件
		/// </summary>
		/// <param name="conn">数据库连接</param>
		/// <param name="sql">SQL语句</param>
		/// <param name="fileName">文件名</param>
		/// <param name="subject">邮件的标题</param>
		/// <param name="emailTo">邮件接收人</param>
		public static void EmailTo(this IDbConnection conn, string sql, string fileName, string subject, IEnumerable<string> emailTo)
		{
			EmailTo(conn, sql, fileName, subject, emailTo, Env.EmailHost, int.Parse(Env.EmailPort), Env.EmailAccount, Env.EmailPassword, Env.EmailDisplayName);
		}

		/// <summary>
		/// 把数据库数据导出到EXCEL并发送邮件
		/// </summary>
		/// <param name="conn">数据库连接</param>
		/// <param name="sql">SQL语句</param>
		/// <param name="fileName">文件名</param>
		/// <param name="subject">邮件的标题</param>
		/// <param name="emailTo">邮件接收人</param>
		/// <param name="emailHost">邮件发送服务地址</param>
		/// <param name="port">邮件发送服务端口</param>
		/// <param name="account">邮件发送服务的用户名</param>
		/// <param name="password">邮件发送服务的密码</param>
		/// <param name="displayName">邮件发送服务的显示名称</param>
		public static void EmailTo(this IDbConnection conn, string sql, string fileName, string subject, string emailTo, string emailHost, int port, string account, string password, string displayName = "DotnetSpider Alert")
		{
			EmailTo(conn, sql, fileName, subject, emailTo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList(), emailHost, port, account, password, displayName);
		}

		/// <summary>
		/// 把数据库数据导出到EXCEL并发送邮件
		/// </summary>
		/// <param name="conn">数据库连接</param>
		/// <param name="sql">SQL语句</param>
		/// <param name="fileName">文件名</param>
		/// <param name="subject">邮件的标题</param>
		/// <param name="emailTo">邮件接收人</param>
		/// <param name="emailHost">邮件发送服务地址</param>
		/// <param name="port">邮件发送服务端口</param>
		/// <param name="account">邮件发送服务的用户名</param>
		/// <param name="password">邮件发送服务的密码</param>
		/// <param name="displayName">邮件发送服务的显示名称</param>
		public static void EmailTo(this IDbConnection conn, string sql, string fileName, string subject, IEnumerable<string> emailTo, string emailHost, int port, string account, string password, string displayName = "DotnetSpider Alert")
		{
			var path = Export(conn, sql, $"{fileName}_{DateTime.Now:yyyyMMddhhmmss}", true);
			EmailTo(path, subject, emailTo, emailHost, port, account, password, displayName);
		}

		/// <summary>
		/// 文件附件发送邮件
		/// </summary>
		/// <typeparam name="T">爬虫实现类型</typeparam>
		/// <param name="file">附件的路径</param>
		public static void EmailTo<T>(string file)
		{
			var description = (Description)typeof(T).GetCustomAttributes(typeof(Description), true).First();
			var emailTo = description.Email?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
			var subject = description.Subject;
			EmailTo(file, subject, emailTo, Env.EmailHost, int.Parse(Env.EmailPort), Env.EmailAccount, Env.EmailPassword);
		}

		/// <summary>
		/// 文件附件发送邮件
		/// </summary>
		/// <param name="file">附件的路径</param>
		/// <param name="subject">邮件的标题</param>
		/// <param name="emailTo">邮件接收人</param>
		/// <param name="emailHost">邮件发送服务地址</param>
		/// <param name="port">邮件发送服务端口</param>
		/// <param name="account">邮件发送服务的用户名</param>
		/// <param name="password">邮件发送服务的密码</param>
		/// <param name="displayName">邮件发送服务的显示名称</param>
		public static void EmailTo(string file, string subject, IEnumerable<string> emailTo, string emailHost, int port, string account, string password, string displayName = "DotnetSpider Alert")
		{
			var message = new MimeMessage();

			message.From.Add(new MailboxAddress(displayName, account));
			foreach (var email in emailTo)
			{
				message.To.Add(new MailboxAddress(email, email));
			}

			message.Subject = subject;

			var attachment = new MimePart("excel", "xlsx")
			{
#if !NET40
				Content = new MimeContent(File.OpenRead(file)),
#else
				ContentObject = new ContentObject(File.OpenRead(file)),
#endif
				ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
				ContentTransferEncoding = ContentEncoding.Base64,
				FileName = Path.GetFileName(file)
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
